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
        public NuDealer() { }
        public static List<CardItem> FetchCards(string CourseCode)
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
                var cards= ParsingService.ParseCourseResponse(responseContent);
                //TODO: update the db
                return cards;
            } 
        }
    }
}
