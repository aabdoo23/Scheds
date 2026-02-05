using Newtonsoft.Json;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.DTOs.SelfService;
using Scheds.Domain.Entities;
using System.Net;
using System.Text;

namespace Scheds.Infrastructure.Services
{
    public class SelfServiceLiveFetchService(IParsingService parsingService,
        ICardItemService cardItemService,
        ICourseBaseRepository courseBaseRepository,
        ICardItemRepository cardItemRepository) : ISelfServiceLiveFetchService
    {
        private readonly IParsingService _parsingService = parsingService ?? throw new ArgumentNullException(nameof(parsingService));
        private readonly ICardItemService _cardItemService = cardItemService ?? throw new ArgumentNullException(nameof(cardItemService));
        private readonly ICourseBaseRepository _courseBaseRepository = courseBaseRepository ?? throw new ArgumentNullException(nameof(courseBaseRepository));
        private readonly ICardItemRepository _cardItemRepository = cardItemRepository ?? throw new ArgumentNullException(nameof(cardItemRepository));

        public async Task<List<CardItem>> FetchCards(string CourseCode)
        {
            var existingCourse = await _courseBaseRepository.GetByIdAsync(CourseCode);

            if (existingCourse != null) Console.WriteLine(DateTime.Now - existingCourse.LastUpdate);
            if (existingCourse != null && DateTime.Now - existingCourse.LastUpdate <= TimeSpan.FromMinutes(10))
            {
                Console.WriteLine("returning cached");
                return await _cardItemRepository.GetCardItemsByCourseCodeAsync(CourseCode);
            }

            var cards = await FetchFromAPI(CourseCode);
            if (cards == null || cards.Count == 0)
            {
                return cards ?? new List<CardItem>();
            }
            foreach (var card in cards)
            {
                await _cardItemService.LiveFetchUpsertAsync(card);
            }
            return cards;
        }

        public async Task<List<CardItem>> FetchCardsSeatModeration(List<string> CourseCodes, List<string> sections)
        {
            var allCards = new List<CardItem>();
            
            var courseSectionPairs = new List<(string courseCode, string section)>();
            for (int i = 0; i < Math.Min(CourseCodes.Count, sections.Count); i++)
            {
                courseSectionPairs.Add((CourseCodes[i], sections[i]));
            }
            
            
            var uniqueCourseCodes = CourseCodes.Distinct().ToList();
            
            foreach (var courseCode in uniqueCourseCodes)
            {
                var cards = await FetchFromAPI(courseCode);

                if (cards == null || cards.Count == 0)
                {
                    continue;
                }

                foreach (var card in cards)
                {
                    await _cardItemService.LiveFetchUpsertAsync(card);
                }

                allCards.AddRange(cards);
            }
            
            var filteredCards = allCards.Where(card => {
                return courseSectionPairs.Any(pair => {
                    bool courseMatch = card.CourseCode.Equals(pair.courseCode, StringComparison.OrdinalIgnoreCase);        
                    string normalizedRequestedSection = pair.section.PadLeft(2, '0');
                    string normalizedCardSection = card.Section.PadLeft(2, '0');
                    bool sectionMatch = normalizedCardSection.Equals(normalizedRequestedSection, StringComparison.OrdinalIgnoreCase);
                    
                    return courseMatch && sectionMatch;
                });
            }).ToList();
            
            return filteredCards;
        }

        public async Task FetchCourseBases(string CourseCode)
        {
            try
            {
                var existingCourse = await _courseBaseRepository.GetByIdAsync(CourseCode);
                if (existingCourse != null) Console.WriteLine(DateTime.Now - existingCourse.LastUpdate);
                if (existingCourse != null && DateTime.Now - existingCourse.LastUpdate <= TimeSpan.FromMinutes(10))
                {
                    Console.WriteLine("returning cached");
                    return;
                }
                var cards = await FetchFromAPI(CourseCode);

                if (cards == null || cards.Count == 0)
                {
                    return;
                }

                foreach (var card in cards)
                {
                    await _cardItemService.LiveFetchUpsertAsync(card);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private async Task<List<CardItem>> FetchFromAPI(string CourseCode)
        {
            try
            {
                var cookieContainer = new CookieContainer();
                using var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = cookieContainer,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    try
                    {
                        using var warmup = new HttpRequestMessage(
                            HttpMethod.Get,
                            "https://register.nu.edu.eg/PowerCampusSelfService/Search/Section");
                        warmup.Headers.TryAddWithoutValidation(
                            "Accept",
                            "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                        warmup.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
                        warmup.Headers.TryAddWithoutValidation(
                            "User-Agent",
                            "Mozilla/5.0 (X11; Ubuntu; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                        await client.SendAsync(warmup);
                    }
                    catch
                    {
                    }

                    var request = new SearchRequest(CourseCode);
                    var jsonRequest = JsonConvert.SerializeObject(request);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    using var postRequest = new HttpRequestMessage(
                        HttpMethod.Post,
                        "https://register.nu.edu.eg/PowerCampusSelfService/Sections/Search")
                    {
                        Content = content
                    };
                    postRequest.Headers.TryAddWithoutValidation("Origin", "https://register.nu.edu.eg");
                    postRequest.Headers.TryAddWithoutValidation(
                        "Referer",
                        "https://register.nu.edu.eg/PowerCampusSelfService/Search/Section");
                    postRequest.Headers.TryAddWithoutValidation("Accept", "application/json");

                    var response = await client.SendAsync(postRequest);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to fetch data from the server. HTTP {response.StatusCode}: {response.ReasonPhrase}");
                    }
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var cards = await _parsingService.ParseSelfServiceResponse(responseContent);
                    return cards;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[NETWORK ERROR] HttpRequestException: {ex.Message}");
                return new List<CardItem>();
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"[TIMEOUT ERROR] Request timed out after 30 seconds: {ex.Message}");
                return new List<CardItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GENERAL ERROR] Unexpected error: {ex.Message}");
                return new List<CardItem>();
            }
        }

    }
}
