using Scheds.Models;
using Scheds.Models.SelfService;
using System.Net.Http.Headers;
using Newtonsoft.Json;

using System.Text.Json.Serialization;
using System.Text;
using Scheds.DAL.Services;
using System.Linq.Expressions;

namespace Scheds.DAL.Repositories
{
    public class NuDealer
    {
        private readonly ParsingService _ParsingService;
        private readonly CardItemRepository CardItemRepository;
        private readonly CourseBaseRepository CourseBaseRepository;
        private readonly CourseScheduleRepository CourseScheduleRepository;
        public NuDealer(ParsingService parsingService, CardItemRepository cardItemRepository, CourseBaseRepository courseBaseRepository, CourseScheduleRepository courseScheduleRepository) { 
            _ParsingService = parsingService;
            CardItemRepository = cardItemRepository;
            CourseBaseRepository = courseBaseRepository;
            CourseScheduleRepository = courseScheduleRepository;

        }
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
                var cards= await _ParsingService.ParseCourseResponse(responseContent);
                
                foreach(var card in cards)
                {
                    CardItemRepository.UpdateCardItemAsync(card);
                    CourseScheduleRepository.UpdateCourseScheduleAsync(card);
                    CourseBase courseBase = new CourseBase(card.CourseCode, card.CourseName);
                    CourseBaseRepository.UpdateCourseBaseAsync(card);

                }
                return cards;
            } 
        }

        public async Task<List<CourseBase>> FetchCoursBases(string CourseCode)
        {
            List<CourseBase> courseBases = new List<CourseBase>();
            try {
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
                    var cards = await _ParsingService.ParseCourseResponse(responseContent);

                    foreach (var card in cards)
                    {
                        Console.WriteLine("Parsed: "+card.ToString());

                        CardItemRepository.UpdateCardItemAsync(card); // Ensure UpdateCardItemAsync handles tracking issues

                        foreach (CourseSchedule schedule in card.Schedule)
                            CourseScheduleRepository.UpdateCourseScheduleAsync(card);

                        var courseBase = new CourseBase(card.CourseCode, card.CourseName);
                        CourseBaseRepository.UpdateCourseBaseAsync(card);
                        courseBases.Add(courseBase);
                    }
                    return courseBases;
                }
                
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return courseBases;
        }


    }
}
