using Microsoft.EntityFrameworkCore;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.DTOs.Admin;
using Scheds.Domain.Entities;
using Scheds.Domain.ViewModels;
using Scheds.Infrastructure.Contexts;

namespace Scheds.Infrastructure.Repositories
{
    public class ScheduleGenerationRepository : IScheduleGenerationRepository
    {
        private readonly SchedsDbContext _context;

        public ScheduleGenerationRepository(SchedsDbContext context)
        {
            _context = context;
        }

        public async Task<ScheduleGeneration> CreateAsync(ScheduleGeneration scheduleGeneration)
        {
            _context.ScheduleGenerations.Add(scheduleGeneration);
            await _context.SaveChangesAsync();
            return scheduleGeneration;
        }

        public async Task<IEnumerable<ScheduleGeneration>> GetAllAsync()
        {
            return await _context.ScheduleGenerations
                .Include(sg => sg.SelectedCourses)
                .Include(sg => sg.SelectedCustomCourses)
                .OrderByDescending(sg => sg.GeneratedAt)
                .ToListAsync();
        }

        public async Task<ScheduleGeneration> GetByIdAsync(int id)
        {
            return await _context.ScheduleGenerations
                .Include(sg => sg.SelectedCourses)
                .Include(sg => sg.SelectedCustomCourses)
                .FirstOrDefaultAsync(sg => sg.Id == id);
        }

        public async Task<int> GetTotalGenerationsCountAsync()
        {
            return await _context.ScheduleGenerations.CountAsync();
        }

        public async Task<IEnumerable<string>> GetMostSelectedCoursesAsync(int topCount = 10)
        {
            var regularCourses = await _context.SelectedCourses
                .GroupBy(sc => sc.CourseCode)
                .Select(g => new { CourseCode = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount)
                .Select(x => $"{x.CourseCode} ({x.Count} times)")
                .ToListAsync();

            var customCourses = await _context.SelectedCustomCourses
                .GroupBy(scc => scc.CourseCode)
                .Select(g => new { CourseCode = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount)
                .Select(x => $"{x.CourseCode} ({x.Count} times)")
                .ToListAsync();

            return regularCourses.Concat(customCourses)
                .GroupBy(x => x.Split(' ')[0])
                .Select(g => g.OrderByDescending(x => int.Parse(x.Split('(')[1].Split(' ')[0])).First())
                .OrderByDescending(x => int.Parse(x.Split('(')[1].Split(' ')[0]))
                .Take(topCount);
        }

        public async Task<IEnumerable<string>> GetMostSelectedCustomizationsAsync(int topCount = 10)
        {
            var customizations = new List<string>();

            var professors = await _context.SelectedCustomCourses
                .Where(scc => !string.IsNullOrEmpty(scc.CustomProfessor))
                .GroupBy(scc => scc.CustomProfessor)
                .Select(g => new { Type = "Professor", Value = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount / 3)
                .ToListAsync();

            var tas = await _context.SelectedCustomCourses
                .Where(scc => !string.IsNullOrEmpty(scc.CustomTA))
                .GroupBy(scc => scc.CustomTA)
                .Select(g => new { Type = "TA", Value = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount / 3)
                .ToListAsync();

            var sections = await _context.SelectedCustomCourses
                .Where(scc => !string.IsNullOrEmpty(scc.CustomMainSection))
                .GroupBy(scc => scc.CustomMainSection)
                .Select(g => new { Type = "Section", Value = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount / 3)
                .ToListAsync();

            return professors.Select(p => $"{p.Type}: {p.Value} ({p.Count} times)")
                .Concat(tas.Select(t => $"{t.Type}: {t.Value} ({t.Count} times)"))
                .Concat(sections.Select(s => $"{s.Type}: {s.Value} ({s.Count} times)"));
        }

        public async Task<IEnumerable<CourseWithCountDTO>> GetMostSelectedCoursesWithCountsAsync(int topCount = 15)
        {
            var regularCourses = await _context.SelectedCourses
                .GroupBy(sc => sc.CourseCode)
                .Select(g => new { CourseCode = g.Key, Count = g.Count() })
                .ToListAsync();

            var customCourses = await _context.SelectedCustomCourses
                .GroupBy(scc => scc.CourseCode)
                .Select(g => new { CourseCode = g.Key, Count = g.Count() })
                .ToListAsync();

            var merged = regularCourses.Concat(customCourses)
                .GroupBy(x => x.CourseCode)
                .Select(g => new CourseWithCountDTO
                {
                    Label = g.Key,
                    Count = g.Max(x => x.Count)
                })
                .OrderByDescending(x => x.Count)
                .Take(topCount)
                .ToList();

            return merged;
        }

        public async Task<IEnumerable<CustomizationWithCountDTO>> GetMostSelectedCustomizationsWithCountsAsync(int topCount = 15)
        {
            var professors = await _context.SelectedCustomCourses
                .Where(scc => !string.IsNullOrEmpty(scc.CustomProfessor))
                .GroupBy(scc => scc.CustomProfessor)
                .Select(g => new { Type = "Professor", Value = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount / 3)
                .ToListAsync();

            var tas = await _context.SelectedCustomCourses
                .Where(scc => !string.IsNullOrEmpty(scc.CustomTA))
                .GroupBy(scc => scc.CustomTA)
                .Select(g => new { Type = "TA", Value = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount / 3)
                .ToListAsync();

            var sections = await _context.SelectedCustomCourses
                .Where(scc => !string.IsNullOrEmpty(scc.CustomMainSection))
                .GroupBy(scc => scc.CustomMainSection)
                .Select(g => new { Type = "Section", Value = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount / 3)
                .ToListAsync();

            return professors.Select(p => new CustomizationWithCountDTO { Label = $"{p.Type}: {p.Value}", Count = p.Count })
                .Concat(tas.Select(t => new CustomizationWithCountDTO { Label = $"{t.Type}: {t.Value}", Count = t.Count }))
                .Concat(sections.Select(s => new CustomizationWithCountDTO { Label = $"{s.Type}: {s.Value}", Count = s.Count }));
        }

        public async Task<IEnumerable<DailyStatsViewModel>> GetGenerationStatsByDateAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.ScheduleGenerations
                .Where(sg => sg.GeneratedAt >= fromDate && sg.GeneratedAt <= toDate)
                .GroupBy(sg => sg.GeneratedAt.Date)
                .Select(g => new DailyStatsViewModel
                {
                    Date = g.Key,
                    Count = g.Count(),
                    TotalSchedulesGenerated = g.Sum(x => x.NumberOfSchedulesGenerated)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetUsageStatisticsAsync()
        {
            var stats = new Dictionary<string, int>();

            stats["Total Generations"] = await _context.ScheduleGenerations.CountAsync();
            stats["Live Data Usage"] = await _context.ScheduleGenerations.CountAsync(sg => sg.UsedLiveData);
            stats["Engineering Students"] = await _context.ScheduleGenerations.CountAsync(sg => sg.IsEngineering);
            stats["Zero Seats Considered"] = await _context.ScheduleGenerations.CountAsync(sg => sg.ConsideredZeroSeats);
            stats["Custom Courses Used"] = await _context.SelectedCustomCourses.CountAsync();
            stats["Regular Courses Used"] = await _context.SelectedCourses.CountAsync();

            return stats;
        }

        public async Task<IEnumerable<RecentGenerationViewModel>> GetRecentGenerationsAsync(int count = 10)
        {
            return await _context.ScheduleGenerations
                .Include(sg => sg.SelectedCourses)
                .Include(sg => sg.SelectedCustomCourses)
                .OrderByDescending(sg => sg.GeneratedAt)
                .Take(count)
                .Select(sg => new RecentGenerationViewModel
                {
                    Id = sg.Id,
                    GeneratedAt = sg.GeneratedAt,
                    NumberOfSchedulesGenerated = sg.NumberOfSchedulesGenerated,
                    TotalCourses = sg.SelectedCourses.Count + sg.SelectedCustomCourses.Count,
                    UsedLiveData = sg.UsedLiveData,
                    IsEngineering = sg.IsEngineering
                })
                .ToListAsync();
        }

        public async Task<MonthlyStatsViewModel> GetMonthlyStatsAsync()
        {
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);

            var currentMonthGenerations = await _context.ScheduleGenerations
                .CountAsync(sg => sg.GeneratedAt >= currentMonthStart);

            var previousMonthGenerations = await _context.ScheduleGenerations
                .CountAsync(sg => sg.GeneratedAt >= previousMonthStart && sg.GeneratedAt < currentMonthStart);

            var currentMonthSchedules = await _context.ScheduleGenerations
                .Where(sg => sg.GeneratedAt >= currentMonthStart)
                .SumAsync(sg => sg.NumberOfSchedulesGenerated);

            var growthPercentage = previousMonthGenerations > 0 
                ? ((double)(currentMonthGenerations - previousMonthGenerations) / previousMonthGenerations) * 100 
                : 0;

            var averageSchedules = currentMonthGenerations > 0 
                ? (double)currentMonthSchedules / currentMonthGenerations 
                : 0;

            return new MonthlyStatsViewModel
            {
                CurrentMonthGenerations = currentMonthGenerations,
                PreviousMonthGenerations = previousMonthGenerations,
                GrowthPercentage = Math.Round(growthPercentage, 1),
                CurrentMonthSchedules = currentMonthSchedules,
                AverageSchedulesPerGeneration = Math.Round(averageSchedules, 1)
            };
        }
    }
}
