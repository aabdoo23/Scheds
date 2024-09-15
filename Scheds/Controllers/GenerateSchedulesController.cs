using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Scheds.DAL.Repositories;
using Scheds.DAL.Services;
using Scheds.Models;

public class GenerateSchedulesController : Controller
{
    private readonly CardItemRepository CardItemRepository;
    public GenerateSchedulesController(CardItemRepository cardItemRepository)
    {
        this.CardItemRepository = cardItemRepository;
    }
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    public async Task<ActionResult> Generate(GenerateRequest request)
    {
        // send a get request to this api '/api/cart/getCartItems' to get the selected items
        // request.selectedItems = new List<CourseBase>();
        // HttpClient client = new HttpClient();
        // client.BaseAddress = new Uri("http://localhost:5254/");
        // HttpResponseMessage response = await client.GetAsync("api/cart/getCartItems");
        // if (response.IsSuccessStatusCode)
        // {
        //     var content = await response.Content.ReadAsStringAsync();
        //     System.Console.WriteLine(content);
        //     request.selectedItems = JsonConvert.DeserializeObject<List<CourseBase>>(content);
        // }
        // else
        // {
        //     return BadRequest("Invalid request data.");
        // }

        System.Console.WriteLine(JsonConvert.SerializeObject(request));

        if (!ModelState.IsValid)
        {
            // Handle invalid model state, return error message or view
            return BadRequest("Invalid request data.");
        }
        List<List<CardItem>> allCardItemsByCourse = new List<List<CardItem>>();

        // Handle custom selected items
        if (request.customSelectedItems != null && request.customSelectedItems.Count > 0)
        {
            foreach (CustomCourseBase customCourse in request.customSelectedItems)
            {
                List<CardItem> cardItems = await CardItemRepository.GetCardItemsByCourseCodeAsync(customCourse.courseCode);

                // Fetch live data if last update was more than 24 hours ago
                //if (DateTime.Now - cardItems[0].LastUpdate > TimeSpan.FromDays(1))
                //{
                //    cardItems = NuDealer.FetchCards(customCourse.courseCode);
                //}

                List<CardItem> customizedCards = new List<CardItem>();

                // Filter by custom main and sub section
                if (!string.IsNullOrEmpty(customCourse.customMainSection) || !string.IsNullOrEmpty(customCourse.customSubSection))
                {
                    foreach (CardItem card in cardItems)
                    {
                        if (card.Section.Substring(0, 2) == customCourse.customMainSection ||
                            card.Section.Substring(0, 2) == customCourse.customSubSection.Substring(0, 2))
                        {
                            if (!customizedCards.Contains(card)) customizedCards.Add(card);
                        }
                    }
                }

                // Filter by custom TA
                if (!string.IsNullOrEmpty(customCourse.customTA))
                {
                    HashSet<string> sections = new HashSet<string>();
                    foreach (CardItem card in cardItems)
                    {
                        if (card.Instructor == customCourse.customTA)
                        {
                            if (!customizedCards.Contains(card))
                            {
                                customizedCards.Add(card);
                                sections.Add(card.Section);
                            }
                        }
                    }

                    // Add other sections related to the TA
                    foreach (CardItem card in cardItems)
                    {
                        if (!customizedCards.Contains(card))
                        {
                            foreach (string section in sections)
                            {
                                if (card.Section.Substring(0, 2) == section.Substring(0, 2))
                                {
                                    customizedCards.Add(card);
                                }
                            }
                        }
                    }
                }

                // Filter by custom professor
                if (!string.IsNullOrEmpty(customCourse.customProfessor))
                {
                    HashSet<string> sections = new HashSet<string>();
                    foreach (CardItem card in cardItems)
                    {
                        if (card.Instructor == customCourse.customProfessor)
                        {
                            if (!customizedCards.Contains(card))
                            {
                                customizedCards.Add(card);
                                sections.Add(card.Section);
                            }
                        }
                    }

                    // Add other sections related to the professor
                    foreach (CardItem card in cardItems)
                    {
                        if (!customizedCards.Contains(card))
                        {
                            foreach (string section in sections)
                            {
                                if (card.Section.Substring(0, 2) == section)
                                {
                                    customizedCards.Add(card);
                                }
                            }
                        }
                    }
                }

                allCardItemsByCourse.Add(customizedCards);
            }
        }

        // Handle general selected items
        if (request.selectedItems != null && request.selectedItems.Count > 0)
        {
            foreach (CourseBase course in request.selectedItems)
            {
                List<CardItem> cardItems = await CardItemRepository.GetCardItemsByCourseCodeAsync(course.CourseCode);

                //if (DateTime.Now - cardItems[0].LastUpdate > TimeSpan.FromDays(1))
                //{
                //    cardItems = NuDealer.FetchCards(course.CourseCode);
                //}

                allCardItemsByCourse.Add(cardItems);
            }
        }

        var generatedSchedules = GenerationHelper.GenerateAllTimetables(allCardItemsByCourse, request);

        var viewModel = new GenerateAndResultViewModel
        {
            FormRequest = request,
            ScheduleResult = generatedSchedules
        };
        return View("Index", viewModel);

    }
}
