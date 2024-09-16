using Scheds.Models;
using Scheds.Models.SelfService;
using System.Net.Http.Headers;
using Newtonsoft.Json;

using System.Text.Json.Serialization;
using System.Text;
using Scheds.DAL.Services;

namespace Scheds.DAL.Repositories
{
    public class NuDealer
    {
        private readonly ParsingService ParsingService;
        private readonly CardItemRepository CardItemRepository;
        private readonly CourseBaseRepository CourseBaseRepository;
        private readonly CourseScheduleRepository CourseScheduleRepository;
        public NuDealer(ParsingService parsingService, CardItemRepository cardItemRepository, CourseBaseRepository courseBaseRepository, CourseScheduleRepository courseScheduleRepository) { 
            ParsingService = parsingService;
            CardItemRepository = cardItemRepository;
            CourseBaseRepository = courseBaseRepository;
            CourseScheduleRepository = courseScheduleRepository;

        }
        public async Task<List<CardItem>> FetchCards(string CourseCode)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Origin", "https://register.nu.edu.eg");
                client.DefaultRequestHeaders.Add("Referer", "https://register.nu.edu.eg/PowerCampusSelfService/Search/Section");
                
                var request = new SearchRequest(CourseCode);
                var jsonRequest= JsonConvert.SerializeObject(request);
                var content= new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = client.PostAsync("https://register.nu.edu.eg/PowerCampusSelfService/Sections/Search", content).Result;
                
                if(!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to fetch data from the server");
                }
                var responseContent = response.Content.ReadAsStringAsync().Result;
                
                var cards= await ParsingService.ParseCourseResponse(responseContent);
                
                foreach(var card in cards)
                {
                    await CardItemRepository.UpdateCardItemAsync(card);
                    foreach(CourseSchedule schedule in card.Schedule)
                        await CourseScheduleRepository.UpdateCourseScheduleAsync(schedule);
                    CourseBase courseBase = new CourseBase(card.CourseCode, card.CourseName);
                    await CourseBaseRepository.UpdateCourseBaseAsync(courseBase);

                }
                return cards;
            } 
        }

        public async Task<List<CourseBase>> FetchCoursBases(string CourseCode)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Origin", "https://register.nu.edu.eg");
                client.DefaultRequestHeaders.Add("Referer", "https://register.nu.edu.eg/PowerCampusSelfService/Search/Section");

                var request = new SearchRequest(CourseCode);
                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = client.PostAsync("https://register.nu.edu.eg/PowerCampusSelfService/Sections/Search", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to fetch data from the server");
                }
                var responseContent = response.Content.ReadAsStringAsync().Result;

                var cards = await ParsingService.ParseCourseResponse(responseContent);
                var courseBases = new List<CourseBase>();

                foreach (var card in cards)
                {
                    await CardItemRepository.UpdateCardItemAsync(card); // Ensure UpdateCardItemAsync handles tracking issues

                    foreach (CourseSchedule schedule in card.Schedule)
                        await CourseScheduleRepository.UpdateCourseScheduleAsync(schedule);

                    var courseBase = new CourseBase(card.CourseCode, card.CourseName);
                    await CourseBaseRepository.UpdateCourseBaseAsync(courseBase);
                    courseBases.Add(courseBase);
                }
                return courseBases;
            }
        }

    }
}
