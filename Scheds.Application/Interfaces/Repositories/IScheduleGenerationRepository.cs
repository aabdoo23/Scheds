using Scheds.Domain.DTOs.Admin;
using Scheds.Domain.Entities;
using Scheds.Domain.ViewModels;

namespace Scheds.Application.Interfaces.Repositories
{
    public interface IScheduleGenerationRepository
    {
        Task<ScheduleGeneration> CreateAsync(ScheduleGeneration scheduleGeneration);
        Task<IEnumerable<ScheduleGeneration>> GetAllAsync();
        Task<ScheduleGeneration> GetByIdAsync(int id);
        Task<int> GetTotalGenerationsCountAsync();
        Task<IEnumerable<string>> GetMostSelectedCoursesAsync(int topCount = 10);
        Task<IEnumerable<string>> GetMostSelectedCustomizationsAsync(int topCount = 10);
        Task<IEnumerable<CourseWithCountDTO>> GetMostSelectedCoursesWithCountsAsync(int topCount = 15);
        Task<IEnumerable<CustomizationWithCountDTO>> GetMostSelectedCustomizationsWithCountsAsync(int topCount = 15);
        Task<IEnumerable<DailyStatsViewModel>> GetGenerationStatsByDateAsync(DateTime fromDate, DateTime toDate);
        Task<Dictionary<string, int>> GetUsageStatisticsAsync();
        Task<IEnumerable<RecentGenerationViewModel>> GetRecentGenerationsAsync(int count = 10);
        Task<MonthlyStatsViewModel> GetMonthlyStatsAsync();
    }
}
