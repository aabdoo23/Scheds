using Newtonsoft.Json;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.DTOs.SelfService;
using Scheds.Domain.Entities;
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

            // Return cached data if recent
            if(existingCourse != null)Console.WriteLine(DateTime.Now - existingCourse.LastUpdate);
            if (existingCourse != null && DateTime.Now - existingCourse.LastUpdate <= TimeSpan.FromMinutes(10))
            {
                Console.WriteLine("returning cached");
                return await _cardItemRepository.GetCardItemsByCourseCodeAsync(CourseCode);
            }

            // Fetch from API and update cache
            var cards = await FetchFromAPI(CourseCode);
            foreach (var card in cards)
            {
                await _cardItemService.LiveFetchUpsertAsync(card);
            }
            return cards;
        }
        
        public async Task FetchCourseBases(string CourseCode)
        {
            try
            {
                var existingCourse = await _courseBaseRepository.GetByIdAsync(CourseCode);

                // Return cached data if recent
                if (existingCourse != null) Console.WriteLine(DateTime.Now - existingCourse.LastUpdate);
                if (existingCourse != null && DateTime.Now - existingCourse.LastUpdate <= TimeSpan.FromMinutes(10))
                {
                    Console.WriteLine("returning cached");
                    return;
                }
                var cards = await FetchFromAPI(CourseCode);

                foreach (var card in cards)
                {
                    Console.WriteLine("Parsed: " + card.ToString());
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
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Origin", "https://register.nu.edu.eg");
                client.DefaultRequestHeaders.Add("Referer", "https://register.nu.edu.eg/PowerCampusSelfService/Registration/Courses");

                var request = new SearchRequest(CourseCode);
                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://register.nu.edu.eg/PowerCampusSelfService/Sections/Search", content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to fetch data from the server");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                var cards = await _parsingService.ParseSelfServiceResponse(responseContent);
                return cards;
            }
        }

    }
}
