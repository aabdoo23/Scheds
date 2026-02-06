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
                        var liveCards = await _selfServiceLiveFetchService.FetchCards(customCourse.CourseCode);
                        if (liveCards.Count > 0) cardItems = liveCards;
                    }

                    var excludedMain = customCourse.EffectiveExcludedMainSections.ToList();
                    var excludedSub = customCourse.EffectiveExcludedSubSections.ToList();
                    var excludedProfessors = customCourse.EffectiveExcludedProfessors.ToList();
                    var excludedTas = customCourse.EffectiveExcludedTAs.ToList();
                    bool hasAnyFilter = excludedMain.Count > 0 || excludedSub.Count > 0 || excludedProfessors.Count > 0 || excludedTas.Count > 0;

                    List<CardItem> customizedCards = hasAnyFilter ? [] : cardItems;

                    if (hasAnyFilter)
                    {
                        IEnumerable<CardItem> candidates = cardItems;

                        if (excludedMain.Count > 0)
                        {
                            var excludedMainSet = excludedMain.ToHashSet();
                            candidates = candidates.Where(c => c.Section == null || c.Section.Length != 2 || !excludedMainSet.Contains(c.Section));
                        }

                        if (excludedSub.Count > 0)
                        {
                            var excludedSubSet = excludedSub.ToHashSet();
                            candidates = candidates.Where(c => c.Section == null || c.Section.Length <= 2 || !excludedSubSet.Contains(c.Section));
                        }

                        if (excludedProfessors.Count > 0)
                        {
                            var excludedProfSet = excludedProfessors.ToHashSet();
                            candidates = candidates.Where(c => c.Instructor == null || !excludedProfSet.Contains(c.Instructor));
                        }

                        if (excludedTas.Count > 0)
                        {
                            var excludedTaSet = excludedTas.ToHashSet();
                            candidates = candidates.Where(c => c.Instructor == null || !excludedTaSet.Contains(c.Instructor));
                        }

                        customizedCards = candidates.ToList();
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
                        var liveCards = await _selfServiceLiveFetchService.FetchCards(course.CourseCode);
                        if (liveCards.Count > 0) cardItems = liveCards;
                    }

                    allCardItemsByCourse.Add(cardItems);
                }
            }

            var generatedSchedules = GenerationUtil.GenerateAllTimetables(allCardItemsByCourse, request);
            
            try
            {
                await StoreAnalyticsAsync(request, generatedSchedules?.Count() ?? 0);
            }
            catch
            {
                // Analytics storage is non-fatal; generation result still returned
            }
            
            return Ok(generatedSchedules);
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
                    var excludedMain = customCourse.EffectiveExcludedMainSections.ToList();
                    var excludedSub = customCourse.EffectiveExcludedSubSections.ToList();
                    var excludedProfessors = customCourse.EffectiveExcludedProfessors.ToList();
                    var excludedTas = customCourse.EffectiveExcludedTAs.ToList();
                    scheduleGeneration.SelectedCustomCourses.Add(new SelectedCustomCourse
                    {
                        CourseCode = customCourse.CourseCode,
                        CourseName = customCourse.CourseName,
                        CustomMainSection = excludedMain.Count > 0 ? string.Join(", ", excludedMain) : null,
                        CustomSubSection = excludedSub.Count > 0 ? string.Join(", ", excludedSub) : null,
                        CustomProfessor = excludedProfessors.Count > 0 ? string.Join(", ", excludedProfessors) : null,
                        CustomTA = excludedTas.Count > 0 ? string.Join(", ", excludedTas) : null
                    });
                }
            }

            await _scheduleGenerationRepository.CreateAsync(scheduleGeneration);
        }
    }
}
