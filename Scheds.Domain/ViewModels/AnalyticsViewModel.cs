using Scheds.Domain.DTOs.Admin;

namespace Scheds.Domain.ViewModels
{
    public class AdminDashboardViewModel
    {
        public Dictionary<string, int> UsageStatistics { get; set; }
        public IEnumerable<string> MostSelectedCourses { get; set; }
        public IEnumerable<string> MostSelectedCustomizations { get; set; }
        public IEnumerable<DailyStatsViewModel> DailyStats { get; set; }
        public IEnumerable<RecentGenerationViewModel> RecentGenerations { get; set; }
        public MonthlyStatsViewModel MonthlyStats { get; set; }
    }

    public class AnalyticsViewModel
    {
        public Dictionary<string, int> UsageStatistics { get; set; }
        public IEnumerable<string> MostSelectedCourses { get; set; }
        public IEnumerable<string> MostSelectedCustomizations { get; set; }
        public IEnumerable<CourseWithCountDTO> MostSelectedCoursesWithCounts { get; set; }
        public IEnumerable<CustomizationWithCountDTO> MostSelectedCustomizationsWithCounts { get; set; }
        public IEnumerable<DailyStatsViewModel> DailyStats { get; set; }
        public MonthlyStatsViewModel MonthlyStats { get; set; }
    }

    public class DailyStatsViewModel
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int TotalSchedulesGenerated { get; set; }
    }

    public class RecentGenerationViewModel
    {
        public int Id { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int NumberOfSchedulesGenerated { get; set; }
        public int TotalCourses { get; set; }
        public bool UsedLiveData { get; set; }
        public bool IsEngineering { get; set; }
    }

    public class MonthlyStatsViewModel
    {
        public int CurrentMonthGenerations { get; set; }
        public int PreviousMonthGenerations { get; set; }
        public double GrowthPercentage { get; set; }
        public int CurrentMonthSchedules { get; set; }
        public double AverageSchedulesPerGeneration { get; set; }
    }
}
