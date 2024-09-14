using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Scheds.DAL.Repositories;
using Scheds.DAL.Services;
using Scheds.Models;
using System.Text;

namespace Scheds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardController
    {
        private readonly CardItemRepository repository;
        public CardController(CardItemRepository repository)
        {
            this.repository = repository;
        }
        [HttpGet]
        public async Task<ActionResult<List<CardItem>>> GetAllCards()
        {
            var cardItems = await repository.GetAllCardItemsAsync();
            return cardItems;
        }
        [HttpGet("byId/{id}")]
        public async Task<ActionResult<CardItem>> GetCard(int id)
        {
            var cardItem = await repository.GetCardItemByIdAsync(id);
            if (cardItem == null)
            {
                return new NotFoundResult();
            }
            return cardItem;
        }

        [HttpGet("{courseCode}")]
        public async Task<ActionResult<List<CardItem>>> GetCardByCourseCode(string courseCode)
        {
            var cardItems = await repository.GetCardItemsByCourseCodeAsync(courseCode);
            if (cardItems == null)
            {
                return new NotFoundResult();
            }
            return cardItems;
        }
        [HttpGet("fetch/{courseCode}")]
        public async Task<ActionResult<List<CardItem>>> FetchCards(string courseCode)
        {
            var cards = new List<CardItem>();
            var client = new HttpClient();
            
            client.DefaultRequestHeaders.Add("Origin", "https://register.nu.edu.eg");
            client.DefaultRequestHeaders.Add("Referer", "https://register.nu.edu.eg/PowerCampusSelfService/Search/Section");
            client.DefaultRequestHeaders.Add("Priority", "u=1, i");

            var postData = new
            {
                sectionSearchParameters = new
                {
                    keywords = courseCode,
                    eventId=courseCode,
                    period = "",
                    registrationtype = "TRAD",
                    session = "",
                    startDateKey = 0,
                    instructorId = "",
                    campusId = "",
                    classLevel = "",
                    college = "",
                    creditType = "",
                    curriculum = "",
                    department = "",
                    endDate = "",
                    endTime = "",
                    eventSubType = "",
                    eventType = "",
                    generalEd = "",
                    meeting = "",
                    nonTradProgram = "",
                    population = "",
                    program = "",
                    startDate = "",
                    startTime = "",
                    status = ""
                },
                startIndex = 0,
                length = 100
            };
            var jsonRequest = JsonConvert.SerializeObject(postData);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://register.nu.edu.eg/PowerCampusSelfService/Sections/Search", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to fetch data from the server");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            //return responseContent;
            cards = ParsingService.ParseCourseResponse(responseContent);
            //TODO: update the db
            return cards;

        }
    }
}
