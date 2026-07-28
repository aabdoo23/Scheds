using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Configuration;
using Scheds.Domain.DTOs.SelfService;
using Scheds.Domain.Entities;
using Scheds.Domain.ValueObjects;
using System.Text;

namespace Scheds.Infrastructure.Services
{
    public class SelfServiceLiveFetchService(
        IParsingService parsingService,
        ICardItemService cardItemService,
        ICourseBaseRepository courseBaseRepository,
        ICardItemRepository cardItemRepository,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IOptions<SeatAlertSettings> seatAlertSettings,
        ILogger<SelfServiceLiveFetchService> logger) : ISelfServiceLiveFetchService
    {
        private const string BaseUrl = "https://register.nu.edu.eg/PowerCampusSelfService/Sections/Search";
        private const string WarmupUrl = "https://register.nu.edu.eg/PowerCampusSelfService/Search/Section";
        private const int BatchSize = 100;

        /// <summary>Freshness window for general course browsing.</summary>
        private static readonly TimeSpan CourseDataTtl = TimeSpan.FromMinutes(10);

        /// <summary>
        /// How long to stop refetching a course code that Self-Service returned nothing for.
        /// </summary>
        private static readonly TimeSpan EmptyResultTtl = TimeSpan.FromMinutes(10);

        private readonly IParsingService _parsingService = parsingService ?? throw new ArgumentNullException(nameof(parsingService));
        private readonly ICardItemService _cardItemService = cardItemService ?? throw new ArgumentNullException(nameof(cardItemService));
        private readonly ICourseBaseRepository _courseBaseRepository = courseBaseRepository ?? throw new ArgumentNullException(nameof(courseBaseRepository));
        private readonly ICardItemRepository _cardItemRepository = cardItemRepository ?? throw new ArgumentNullException(nameof(cardItemRepository));
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        private readonly SeatAlertSettings _seatAlertSettings = seatAlertSettings?.Value ?? throw new ArgumentNullException(nameof(seatAlertSettings));
        private readonly ILogger<SelfServiceLiveFetchService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<List<CardItem>> FetchCards(string CourseCode)
        {
            if (await IsFreshAsync(CourseCode, CourseDataTtl))
                return await _cardItemRepository.GetCardItemsByCourseCodeAsync(CourseCode);

            try
            {
                var cards = await FetchFromAPI(CourseCode);
                await _cardItemService.LiveFetchRefreshCourseAsync(CourseCode, cards);
                return cards;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Live fetch failed for {CourseCode}; serving stored data", CourseCode);
                return await _cardItemRepository.GetCardItemsByCourseCodeAsync(CourseCode);
            }
        }

        public async Task<List<CardItem>> FetchSeatData(IReadOnlyCollection<CourseSection> requested)
        {
            var wanted = requested.ToHashSet();
            if (wanted.Count == 0)
                return new List<CardItem>();

            var seatDataTtl = TimeSpan.FromSeconds(Math.Max(5, _seatAlertSettings.SeatDataFreshnessSeconds));
            var courseCodes = wanted
                .Select(pair => pair.CourseCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var allCards = new List<CardItem>();
            var toFetch = new List<string>();

            // Phase 1 — decide what is stale. Sequential: these are database reads.
            foreach (var courseCode in courseCodes)
            {
                if (_memoryCache.TryGetValue(EmptyResultKey(courseCode), out _))
                {
                    _logger.LogDebug("Skipping {CourseCode}: Self-Service recently returned no sections", courseCode);
                    continue;
                }

                if (await IsFreshAsync(courseCode, seatDataTtl))
                {
                    allCards.AddRange(await _cardItemRepository.GetSeatDataByCourseCodeAsync(courseCode));
                    continue;
                }

                toFetch.Add(courseCode);
            }

            if (toFetch.Count == 0)
                return FilterToRequested(allCards, wanted);

            // Phase 2 — download in parallel. Pure HTTP, no database access.
            var downloads = await DownloadCoursesAsync(toFetch);

            // Phase 3 — parse and persist. Sequential: parsing resolves instructors and the
            // refresh writes cards, and both share one DbContext, which is not thread safe.
            foreach (var courseCode in toFetch)
            {
                if (!downloads.TryGetValue(courseCode, out var pages) || pages == null)
                {
                    _logger.LogWarning("Download failed for {CourseCode}; serving stored seat data", courseCode);
                    allCards.AddRange(await _cardItemRepository.GetSeatDataByCourseCodeAsync(courseCode));
                    continue;
                }

                var cards = await ParsePagesAsync(pages);

                if (cards.Count == 0)
                {
                    // Nothing came back, so no CourseBase row gets written to mark freshness —
                    // which is why an unknown or delisted code used to be re-scraped on every
                    // single cycle, forever. Remember the empty result instead.
                    //
                    // Deliberately does not refresh the course: wiping stored cards on the back of
                    // one empty response would let an upstream hiccup delete real data. The cards
                    // are also not returned, so a delisted course cannot raise a phantom alert.
                    _memoryCache.Set(EmptyResultKey(courseCode), true, EmptyResultTtl);
                    _logger.LogInformation(
                        "{CourseCode} returned no sections; suppressing refetch for {Minutes} minutes",
                        courseCode, EmptyResultTtl.TotalMinutes);
                    continue;
                }

                await _cardItemService.LiveFetchRefreshCourseAsync(courseCode, cards);
                allCards.AddRange(cards);
            }

            return FilterToRequested(allCards, wanted);
        }

        public async Task<bool> FetchCourseBases(string CourseCode)
        {
            try
            {
                if (await IsFreshAsync(CourseCode, CourseDataTtl))
                    return true;

                var cards = await FetchFromAPI(CourseCode);
                await _cardItemService.LiveFetchRefreshCourseAsync(CourseCode, cards);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Course base fetch failed for {CourseCode}", CourseCode);
                return false;
            }
        }

        private async Task<bool> IsFreshAsync(string courseCode, TimeSpan ttl)
        {
            var existingCourse = await _courseBaseRepository.GetByIdAsync(courseCode);
            return existingCourse != null && DateTime.UtcNow - existingCourse.LastUpdate <= ttl;
        }

        private static List<CardItem> FilterToRequested(
            IEnumerable<CardItem> cards, HashSet<CourseSection> wanted)
        {
            return cards
                .Where(card => CourseSection.Create(card.CourseCode, card.Section) is { } pair && wanted.Contains(pair))
                .ToList();
        }

        /// <summary>
        /// Downloads every page for each course concurrently. The value is null when that course
        /// failed, so the caller can fall back to stored data per course rather than losing the
        /// whole batch.
        /// </summary>
        private async Task<Dictionary<string, List<string>?>> DownloadCoursesAsync(List<string> courseCodes)
        {
            var maxConcurrency = Math.Clamp(_seatAlertSettings.MaxConcurrentFetches, 1, 8);
            var client = _httpClientFactory.CreateClient("SelfServiceApi");

            // One warmup for the whole batch. It exists to establish session cookies, which are
            // held on the shared handler, so doing it per course was pure overhead.
            await WarmupAsync(client);

            using var gate = new SemaphoreSlim(maxConcurrency);
            var downloads = courseCodes.Select(async courseCode =>
            {
                await gate.WaitAsync();
                try
                {
                    return (courseCode, pages: await TryDownloadCoursePagesAsync(client, courseCode));
                }
                finally
                {
                    gate.Release();
                }
            });

            var results = await Task.WhenAll(downloads);

            var byCourse = new Dictionary<string, List<string>?>(StringComparer.OrdinalIgnoreCase);
            foreach (var (courseCode, pages) in results)
                byCourse[courseCode] = pages;

            _logger.LogInformation("Downloaded {Succeeded}/{Total} course(s) at up to {Concurrency} at a time",
                results.Count(r => r.pages != null), courseCodes.Count, maxConcurrency);

            return byCourse;
        }

        private async Task<List<string>?> TryDownloadCoursePagesAsync(HttpClient client, string courseCode)
        {
            try
            {
                return await DownloadCoursePagesAsync(client, courseCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download {CourseCode} from Self-Service", courseCode);
                return null;
            }
        }

        /// <summary>Raw response bodies for one course, walking pagination.</summary>
        private async Task<List<string>> DownloadCoursePagesAsync(HttpClient client, string courseCode)
        {
            var pages = new List<string>();
            var startIndex = 0;

            while (true)
            {
                var request = new SearchRequest(courseCode) { startIndex = startIndex, length = BatchSize };
                var jsonRequest = JsonConvert.SerializeObject(request);
                using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                using var response = await client.PostAsync(BaseUrl, content);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException(
                        $"Failed to fetch data from the server. HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");

                var responseContent = await response.Content.ReadAsStringAsync();

                // Counting sections here keeps pagination in the download phase, so no database
                // work happens while requests are running in parallel.
                var sectionCount = _parsingService.CountSections(responseContent);
                if (sectionCount == 0)
                    break;

                pages.Add(responseContent);

                if (sectionCount < BatchSize)
                    break;

                startIndex += BatchSize;
            }

            return pages;
        }

        private async Task<List<CardItem>> ParsePagesAsync(IEnumerable<string> pages)
        {
            var cards = new List<CardItem>();
            foreach (var page in pages)
                cards.AddRange(await _parsingService.ParseSelfServiceResponse(page));
            return cards;
        }

        private async Task<List<CardItem>> FetchFromAPI(string CourseCode)
        {
            var client = _httpClientFactory.CreateClient("SelfServiceApi");
            await WarmupAsync(client);

            var pages = await DownloadCoursePagesAsync(client, CourseCode);
            return await ParsePagesAsync(pages);
        }

        private static string EmptyResultKey(string courseCode) =>
            $"selfservice:empty:{courseCode.ToUpperInvariant()}";

        private static async Task WarmupAsync(HttpClient client)
        {
            try
            {
                using var warmup = new HttpRequestMessage(HttpMethod.Get, WarmupUrl);
                using var response = await client.SendAsync(warmup);
            }
            catch { }
        }
    }
}
