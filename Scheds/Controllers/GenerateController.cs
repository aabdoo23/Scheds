using Microsoft.AspNetCore.Mvc;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.DTOs;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Util;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenerateController(ICardItemRepository cardItemRepository, ISelfServiceLiveFetchService selfServiceLiveFetchService) : Controller
    {
        private readonly ICardItemRepository _cardItemRepository = cardItemRepository
            ?? throw new ArgumentNullException(nameof(cardItemRepository));
        private readonly ISelfServiceLiveFetchService _selfServiceLiveFetchService = selfServiceLiveFetchService
            ?? throw new ArgumentNullException(nameof(selfServiceLiveFetchService));


        [HttpPost]
        public async Task<IActionResult> Generate(GenerateRequestDTO request)
        {
            Console.WriteLine(request.ToString());
            List<List<CardItem>> allCardItemsByCourse = [];

            // Handle custom selected items
            if (request.CustomSelectedItems != null && request.CustomSelectedItems.Count > 0)
            {
                foreach (CustomCourseBaseDTO customCourse in request.CustomSelectedItems)
                {
                    List<CardItem> cardItems = await _cardItemRepository.GetCardItemsByCourseCodeAsync(customCourse.CourseCode);

                    if (request.UseLiveData)
                    {
                        cardItems = await _selfServiceLiveFetchService.FetchCards(customCourse.CourseCode);
                    }

                    List<CardItem> customizedCards = [];

                    // Filter by custom main and sub section
                    if (!string.IsNullOrEmpty(customCourse.CustomMainSection) || !string.IsNullOrEmpty(customCourse.CustomSubSection))
                    {
                        foreach (CardItem card in cardItems)
                        {
                            if (card.Section[..2] == customCourse.CustomMainSection ||
                                card.Section[..2] == customCourse.CustomSubSection[..2])
                            {
                                if (!customizedCards.Contains(card)) customizedCards.Add(card);
                            }
                        }
                    }

                    // Filter by custom TA
                    if (!string.IsNullOrEmpty(customCourse.CustomTA))
                    {
                        HashSet<string> sections = [];
                        foreach (CardItem card in cardItems)
                        {
                            if (card.Instructor == customCourse.CustomTA)
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
                                    if (card.Section[..2] == section[..2])
                                    {
                                        customizedCards.Add(card);
                                    }
                                }
                            }
                        }
                    }

                    // Filter by custom professor
                    if (!string.IsNullOrEmpty(customCourse.CustomProfessor))
                    {
                        HashSet<string> sections = [];
                        foreach (CardItem card in cardItems)
                        {
                            if (card.Instructor == customCourse.CustomProfessor)
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
                                    if (card.Section[..2] == section)
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
            if (request.SelectedItems != null && request.SelectedItems.Count > 0)
            {
                foreach (CourseBase course in request.SelectedItems)
                {
                    List<CardItem> cardItems = await _cardItemRepository.GetCardItemsByCourseCodeAsync(course.CourseCode);

                    if (request.UseLiveData)
                    {
                        cardItems = await _selfServiceLiveFetchService.FetchCards(course.CourseCode);
                    }

                    allCardItemsByCourse.Add(cardItems);
                }
            }

            var generatedSchedules = GenerationUtil.GenerateAllTimetables(allCardItemsByCourse, request);
            return ViewComponent("AllSchedulesViewComponent", generatedSchedules);
        }
    }
}
