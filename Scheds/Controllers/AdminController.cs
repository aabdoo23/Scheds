using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Configuration;
using Scheds.Domain.ViewModels;

namespace Scheds.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IScheduleGenerationRepository _scheduleGenerationRepository;
        private readonly AdminSettings _adminSettings;
        private const string AdminSessionKey = "AdminAuthenticated";

        public AdminController(IScheduleGenerationRepository scheduleGenerationRepository, IOptions<AdminSettings> adminSettings)
        {
            _scheduleGenerationRepository = scheduleGenerationRepository;
            _adminSettings = adminSettings.Value;
        }

        public IActionResult Login()
        {
            if (IsAuthenticated())
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string password)
        {
            if (password == _adminSettings.Password)
            {
                HttpContext.Session.SetString(AdminSessionKey, "true");
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Invalid password";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove(AdminSessionKey);
            return RedirectToAction("Login");
        }

        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetString(AdminSessionKey) == "true";
        }

        private IActionResult CheckAuthentication()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            var usageStats = await _scheduleGenerationRepository.GetUsageStatisticsAsync();
            var mostSelectedCourses = await _scheduleGenerationRepository.GetMostSelectedCoursesAsync(10);
            var mostSelectedCustomizations = await _scheduleGenerationRepository.GetMostSelectedCustomizationsAsync(10);
            var recentGenerations = await _scheduleGenerationRepository.GetRecentGenerationsAsync(5);
            var monthlyStats = await _scheduleGenerationRepository.GetMonthlyStatsAsync();
            
            var fromDate = DateTime.UtcNow.AddDays(-7); // Last 7 days for chart
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

            return View(viewModel);
        }

        public async Task<IActionResult> Analytics()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            var stats = await _scheduleGenerationRepository.GetUsageStatisticsAsync();
            var mostSelectedCourses = await _scheduleGenerationRepository.GetMostSelectedCoursesAsync(15);
            var mostSelectedCustomizations = await _scheduleGenerationRepository.GetMostSelectedCustomizationsAsync(15);
            
            var fromDate = DateTime.UtcNow.AddDays(-30);
            var toDate = DateTime.UtcNow;
            var dailyStats = await _scheduleGenerationRepository.GetGenerationStatsByDateAsync(fromDate, toDate);

            var viewModel = new AnalyticsViewModel
            {
                UsageStatistics = stats,
                MostSelectedCourses = mostSelectedCourses,
                MostSelectedCustomizations = mostSelectedCustomizations,
                DailyStats = dailyStats
            };

            return View(viewModel);
        }

        public async Task<IActionResult> GenerationHistory()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            var generations = await _scheduleGenerationRepository.GetAllAsync();
            return View(generations);
        }

        public async Task<IActionResult> GenerationDetails(int id)
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            var generation = await _scheduleGenerationRepository.GetByIdAsync(id);
            if (generation == null)
            {
                return NotFound();
            }
            return View(generation);
        }
    }
}
