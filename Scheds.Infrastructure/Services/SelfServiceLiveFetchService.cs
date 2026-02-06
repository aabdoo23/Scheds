using Newtonsoft.Json;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.DTOs.SelfService;
using Scheds.Domain.Entities;
using System.Text;

namespace Scheds.Infrastructure.Services
{
    public class SelfServiceLiveFetchService(
        IParsingService parsingService,
        ICardItemService cardItemService,
        ICourseBaseRepository courseBaseRepository,
        ICardItemRepository cardItemRepository,
        IHttpClientFactory httpClientFactory) : ISelfServiceLiveFetchService
    {
        private const string BaseUrl = "https://register.nu.edu.eg/PowerCampusSelfService/Sections/Search";
        private const string WarmupUrl = "https://register.nu.edu.eg/PowerCampusSelfService/Search/Section";
        private const int BatchSize = 100;

        private readonly IParsingService _parsingService = parsingService ?? throw new ArgumentNullException(nameof(parsingService));
        private readonly ICardItemService _cardItemService = cardItemService ?? throw new ArgumentNullException(nameof(cardItemService));
        private readonly ICourseBaseRepository _courseBaseRepository = courseBaseRepository ?? throw new ArgumentNullException(nameof(courseBaseRepository));
        private readonly ICardItemRepository _cardItemRepository = cardItemRepository ?? throw new ArgumentNullException(nameof(cardItemRepository));
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

        public async Task<List<CardItem>> FetchCards(string CourseCode)
        {
            var existingCourse = await _courseBaseRepository.GetByIdAsync(CourseCode);
            if (existingCourse != null && DateTime.UtcNow - existingCourse.LastUpdate <= TimeSpan.FromMinutes(10))
                return await _cardItemRepository.GetCardItemsByCourseCodeAsync(CourseCode);

            try
            {
                var cards = await FetchFromAPI(CourseCode);
                await _cardItemService.LiveFetchRefreshCourseAsync(CourseCode, cards);
                return cards;
            }
            catch
            {
                return await _cardItemRepository.GetCardItemsByCourseCodeAsync(CourseCode);
            }
        }

        public async Task<List<CardItem>> FetchCardsSeatModeration(List<string> CourseCodes, List<string> sections)
        {
            var allCards = new List<CardItem>();
            var courseSectionPairs = new List<(string courseCode, string section)>();
            for (var i = 0; i < Math.Min(CourseCodes.Count, sections.Count); i++)
                courseSectionPairs.Add((CourseCodes[i], sections[i]));

            var uniqueCourseCodes = CourseCodes.Distinct().ToList();
            foreach (var courseCode in uniqueCourseCodes)
            {
                var existingCourse = await _courseBaseRepository.GetByIdAsync(courseCode);
                if (existingCourse != null && DateTime.UtcNow - existingCourse.LastUpdate <= TimeSpan.FromMinutes(10))
                {
                    var cached = await _cardItemRepository.GetCardItemsByCourseCodeAsync(courseCode);
                    allCards.AddRange(cached);
                    continue;
                }
                try
                {
                    var cards = await FetchFromAPI(courseCode);
                    await _cardItemService.LiveFetchRefreshCourseAsync(courseCode, cards);
                    allCards.AddRange(cards);
                }
                catch
                {
                    var cached = await _cardItemRepository.GetCardItemsByCourseCodeAsync(courseCode);
                    allCards.AddRange(cached);
                }
            }

            return allCards.Where(card =>
                courseSectionPairs.Any(pair =>
                    card.CourseCode.Equals(pair.courseCode, StringComparison.OrdinalIgnoreCase) &&
                    card.Section.PadLeft(2, '0').Equals(pair.section.PadLeft(2, '0'), StringComparison.OrdinalIgnoreCase))).ToList();
        }

        public async Task<bool> FetchCourseBases(string CourseCode)
        {
            try
            {
                var existingCourse = await _courseBaseRepository.GetByIdAsync(CourseCode);
                if (existingCourse != null && DateTime.UtcNow - existingCourse.LastUpdate <= TimeSpan.FromMinutes(10))
                    return true;

                var cards = await FetchFromAPI(CourseCode);
                await _cardItemService.LiveFetchRefreshCourseAsync(CourseCode, cards);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<List<CardItem>> FetchFromAPI(string CourseCode)
        {
            var client = _httpClientFactory.CreateClient("SelfServiceApi");
            await WarmupAsync(client);

            var allCards = new List<CardItem>();
            var startIndex = 0;
            while (true)
            {
                var request = new SearchRequest(CourseCode) { startIndex = startIndex, length = BatchSize };
                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(BaseUrl, content);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Failed to fetch data from the server. HTTP {response.StatusCode}: {response.ReasonPhrase}");

                var responseContent = await response.Content.ReadAsStringAsync();
                var cards = await _parsingService.ParseSelfServiceResponse(responseContent);
                if (cards.Count == 0)
                    break;
                allCards.AddRange(cards);
                if (cards.Count < BatchSize)
                    break;
                startIndex += BatchSize;
            }
            return allCards;
        }

        private static async Task WarmupAsync(HttpClient client)
        {
            try
            {
                using var warmup = new HttpRequestMessage(HttpMethod.Get, WarmupUrl);
                await client.SendAsync(warmup);
            }
            catch { }
        }

    }
}
