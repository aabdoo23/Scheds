using Microsoft.AspNetCore.Mvc;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.DTOs;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Util;
using System.Text.Json;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenerateController : Controller
    {
        private readonly ICardItemRepository _cardItemRepository;
        private readonly ISelfServiceLiveFetchService _selfServiceLiveFetchService;
        private readonly IScheduleGenerationRepository _scheduleGenerationRepository;

        public GenerateController(
            ICardItemRepository cardItemRepository, 
            ISelfServiceLiveFetchService selfServiceLiveFetchService,
            IScheduleGenerationRepository scheduleGenerationRepository)
        {
            _cardItemRepository = cardItemRepository ?? throw new ArgumentNullException(nameof(cardItemRepository));
            _selfServiceLiveFetchService = selfServiceLiveFetchService ?? throw new ArgumentNullException(nameof(selfServiceLiveFetchService));
            _scheduleGenerationRepository = scheduleGenerationRepository ?? throw new ArgumentNullException(nameof(scheduleGenerationRepository));
        }


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
            
            // Store analytics data
            await StoreAnalyticsAsync(request, generatedSchedules?.Count() ?? 0);
            
            return ViewComponent("AllSchedulesViewComponent", generatedSchedules);
        }

        private async Task StoreAnalyticsAsync(GenerateRequestDTO request, int numberOfSchedulesGenerated)
        {
            var scheduleGeneration = new ScheduleGeneration
            {
                GeneratedAt = DateTime.UtcNow,
                NumberOfSchedulesGenerated = numberOfSchedulesGenerated,
                UsedLiveData = request.UseLiveData,
                ConsideredZeroSeats = request.ConsiderZeroSeats,
                IsEngineering = request.IsEngineering,
                MinimumNumberOfItemsPerDay = request.MinimumNumberOfItemsPerDay,
                LargestAllowedGap = request.LargestAllowedGap,
                NumberOfDays = request.NumberOfDays,
                IsNumberOfDaysSelected = request.IsNumberOfDaysSelected,
                DaysStart = request.DaysStart,
                DaysEnd = request.DaysEnd,
                SelectedDaysJson = JsonSerializer.Serialize(request.SelectedDays),
                ClientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
                SelectedCourses = new List<SelectedCourse>(),
                SelectedCustomCourses = new List<SelectedCustomCourse>()
            };

            // Add regular selected courses
            if (request.SelectedItems != null)
            {
                foreach (var course in request.SelectedItems)
                {
                    scheduleGeneration.SelectedCourses.Add(new SelectedCourse
                    {
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName
                    });
                }
            }

            // Add custom selected courses
            if (request.CustomSelectedItems != null)
            {
                foreach (var customCourse in request.CustomSelectedItems)
                {
                    scheduleGeneration.SelectedCustomCourses.Add(new SelectedCustomCourse
                    {
                        CourseCode = customCourse.CourseCode,
                        CourseName = customCourse.CourseName,
                        CustomMainSection = customCourse.CustomMainSection,
                        CustomSubSection = customCourse.CustomSubSection,
                        CustomProfessor = customCourse.CustomProfessor,
                        CustomTA = customCourse.CustomTA
                    });
                }
            }

            await _scheduleGenerationRepository.CreateAsync(scheduleGeneration);
        }
    }
}
