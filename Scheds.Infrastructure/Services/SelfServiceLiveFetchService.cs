using Newtonsoft.Json;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.DTOs.SelfService;
using Scheds.Domain.Entities;
using System.Text;

namespace Scheds.Infrastructure.Services
{
    public class SelfServiceLiveFetchService(IParsingService parsingService, ICardItemService cardItemService) : ISelfServiceLiveFetchService
    {
        private readonly IParsingService _parsingService = parsingService ?? throw new ArgumentNullException(nameof(parsingService));
        private readonly ICardItemService _cardItemService = cardItemService ?? throw new ArgumentNullException(nameof(cardItemService));

        public async Task<List<CardItem>> FetchCards(string CourseCode)
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

                foreach (var card in cards)
                {
                    await _cardItemService.LiveFetchUpsertAsync(card);
                }
                return cards;
            }
        }

        public async Task<List<CourseBase>> FetchCourseBases(string CourseCode)
        {
            List<CourseBase> courseBases = [];
            try
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

                    foreach (var card in cards)
                    {
                        Console.WriteLine("Parsed: " + card.ToString());

                        await _cardItemService.LiveFetchUpsertAsync(card);
                    }
                    return courseBases;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return courseBases;
        }
    }
}
