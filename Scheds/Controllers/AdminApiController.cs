using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Configuration;
using Scheds.Domain.DTOs.Admin;
using Scheds.Domain.Entities;
using Scheds.Domain.ViewModels;
using Scheds.MVC.Filters;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminApiController : ControllerBase
    {
        private const string AdminSessionKey = "AdminAuthenticated";
        private readonly IScheduleGenerationRepository _scheduleGenerationRepository;
        private readonly AdminSettings _adminSettings;

        public AdminApiController(
            IScheduleGenerationRepository scheduleGenerationRepository,
            IOptions<AdminSettings> adminSettings)
        {
            _scheduleGenerationRepository = scheduleGenerationRepository;
            _adminSettings = adminSettings.Value;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AdminLoginRequest request)
        {
            if (request?.Password == _adminSettings.Password)
            {
                HttpContext.Session.SetString(AdminSessionKey, "true");
                return Ok();
            }
            return Unauthorized();
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(AdminSessionKey);
            return Ok();
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            if (HttpContext.Session.GetString(AdminSessionKey) != "true")
                return Unauthorized();
            return Ok();
        }

        [HttpGet("dashboard")]
        [AdminAuthorize]
        public async Task<IActionResult> Dashboard()
        {
            var usageStats = await _scheduleGenerationRepository.GetUsageStatisticsAsync();
            var mostSelectedCourses = await _scheduleGenerationRepository.GetMostSelectedCoursesAsync(10);
            var mostSelectedCustomizations = await _scheduleGenerationRepository.GetMostSelectedCustomizationsAsync(10);
            var recentGenerations = await _scheduleGenerationRepository.GetRecentGenerationsAsync(5);
            var monthlyStats = await _scheduleGenerationRepository.GetMonthlyStatsAsync();
            var fromDate = DateTime.UtcNow.AddDays(-7);
            var toDate = DateTime.UtcNow;
            var dailyStats = await _scheduleGenerationRepository.GetGenerationStatsByDateAsync(fromDate, toDate);

            var viewModel = new AdminDashboardViewModel
            {
                UsageStatistics = usageStats,
                MostSelectedCourses = mostSelectedCourses,
                MostSelectedCustomizations = mostSelectedCustomizations,
                DailyStats = dailyStats,
                RecentGenerations = recentGenerations,
                MonthlyStats = monthlyStats
            };

            return Ok(viewModel);
        }

        [HttpGet("analytics")]
        [AdminAuthorize]
        public async Task<IActionResult> Analytics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var toDate = to ?? DateTime.UtcNow;
            var fromDate = from ?? toDate.AddDays(-30);
            if (fromDate > toDate)
                (fromDate, toDate) = (toDate, fromDate);

            var stats = await _scheduleGenerationRepository.GetUsageStatisticsAsync();
            var mostSelectedCourses = await _scheduleGenerationRepository.GetMostSelectedCoursesAsync(15);
            var mostSelectedCustomizations = await _scheduleGenerationRepository.GetMostSelectedCustomizationsAsync(15);
            var mostSelectedCoursesWithCounts = await _scheduleGenerationRepository.GetMostSelectedCoursesWithCountsAsync(15);
            var mostSelectedCustomizationsWithCounts = await _scheduleGenerationRepository.GetMostSelectedCustomizationsWithCountsAsync(15);
            var dailyStats = await _scheduleGenerationRepository.GetGenerationStatsByDateAsync(fromDate, toDate);
            var monthlyStats = await _scheduleGenerationRepository.GetMonthlyStatsAsync();

            var viewModel = new AnalyticsViewModel
            {
                UsageStatistics = stats,
                MostSelectedCourses = mostSelectedCourses,
                MostSelectedCustomizations = mostSelectedCustomizations,
                MostSelectedCoursesWithCounts = mostSelectedCoursesWithCounts,
                MostSelectedCustomizationsWithCounts = mostSelectedCustomizationsWithCounts,
                DailyStats = dailyStats,
                MonthlyStats = monthlyStats
            };

            return Ok(viewModel);
        }

        [HttpGet("generations")]
        [AdminAuthorize]
        public async Task<IActionResult> GetGenerations()
        {
            var generations = await _scheduleGenerationRepository.GetAllAsync();
            var dtos = generations.Select(MapToListItem).ToList();
            return Ok(dtos);
        }

        [HttpGet("generations/{id:int}")]
        [AdminAuthorize]
        public async Task<IActionResult> GetGeneration(int id)
        {
            var generation = await _scheduleGenerationRepository.GetByIdAsync(id);
            if (generation == null)
                return NotFound();
            return Ok(MapToDetail(generation));
        }

        private static GenerationListItemDTO MapToListItem(ScheduleGeneration sg)
        {
            return new GenerationListItemDTO
            {
                Id = sg.Id,
                GeneratedAt = sg.GeneratedAt,
                NumberOfSchedulesGenerated = sg.NumberOfSchedulesGenerated,
                SelectedCoursesCount = sg.SelectedCourses?.Count ?? 0,
                SelectedCustomCoursesCount = sg.SelectedCustomCourses?.Count ?? 0,
                UsedLiveData = sg.UsedLiveData,
                IsEngineering = sg.IsEngineering
            };
        }

        private static GenerationDetailDTO MapToDetail(ScheduleGeneration sg)
        {
            return new GenerationDetailDTO
            {
                Id = sg.Id,
                GeneratedAt = sg.GeneratedAt,
                NumberOfSchedulesGenerated = sg.NumberOfSchedulesGenerated,
                UsedLiveData = sg.UsedLiveData,
                ConsideredZeroSeats = sg.ConsideredZeroSeats,
                IsEngineering = sg.IsEngineering,
                MinimumNumberOfItemsPerDay = sg.MinimumNumberOfItemsPerDay,
                LargestAllowedGap = sg.LargestAllowedGap,
                NumberOfDays = sg.NumberOfDays,
                IsNumberOfDaysSelected = sg.IsNumberOfDaysSelected,
                DaysStart = sg.DaysStart ?? "",
                DaysEnd = sg.DaysEnd ?? "",
                SelectedDaysJson = sg.SelectedDaysJson,
                ClientIpAddress = sg.ClientIpAddress,
                UserAgent = sg.UserAgent,
                SelectedCourses = (sg.SelectedCourses ?? []).Select(c => new SelectedCourseDTO
                {
                    CourseCode = c.CourseCode ?? "",
                    CourseName = c.CourseName ?? ""
                }).ToList(),
                SelectedCustomCourses = (sg.SelectedCustomCourses ?? []).Select(c => new SelectedCustomCourseDTO
                {
                    CourseCode = c.CourseCode ?? "",
                    CourseName = c.CourseName ?? "",
                    CustomMainSection = c.CustomMainSection,
                    CustomSubSection = c.CustomSubSection,
                    CustomProfessor = c.CustomProfessor,
                    CustomTA = c.CustomTA
                }).ToList()
            };
        }
    }

    public class AdminLoginRequest
    {
        public string? Password { get; set; }
    }
}
