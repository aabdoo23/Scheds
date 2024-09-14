using Microsoft.AspNetCore.Mvc;
using Scheds.DAL.Repositories;
using Scheds.DAL.Services;
using Scheds.Model;

namespace Scheds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenerateController : Controller
    {
        private readonly CardItemRepository CardItemRepository;
        public GenerateController(CardItemRepository cardItemRepository)
        {
            this.CardItemRepository = cardItemRepository;
        }


        [HttpPost]
        public async Task<ActionResult<List<List<ReturnedCardItem>>>> Generate(GenerateRequest request)
        {
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

            List<List<ReturnedCardItem>> generatedTest = GenerationHelper.GenerateAllTimetables(allCardItemsByCourse, request);

            // Return the result
            return Ok(generatedTest);
        }

    }
}
