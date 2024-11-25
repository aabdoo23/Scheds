using Microsoft.EntityFrameworkCore;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Contexts;
using Scheds.Infrastructure.Repositories.Common;

namespace Scheds.Infrastructure.Repositories
{
    public class CourseScheduleRepository(SchedsDbContext context) : BaseRepository<CourseSchedule>(context), ICourseScheduleRepository
    {
        private readonly SchedsDbContext _context = context
            ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<CourseSchedule>> GetCourseSchedulesByCardIdAsync(string cardId)
        {
            return await _context.CourseSchedules
                .Where(schedule => schedule.CardItemId == cardId)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllRoomsAsync()
        {
            return await _context.CourseSchedules
            .Select(schedule => schedule.Location)
            .Where(room => !string.IsNullOrWhiteSpace(room) && room != "Online")
            .Distinct()
            .ToListAsync();
        }

        public async Task<List<string>> GetOccupiedRoomsAtDayTimeAsync(string dayOfWeek, string time)
        {
            var t = TimeSpan.Parse(time);
            return await _context.CourseSchedules
                .Where(schedule => schedule.DayOfWeek == dayOfWeek &&
                    t >= schedule.StartTime &&
                    t < schedule.EndTime)
                .Select(schedule => schedule.Location).ToListAsync();
        }
    }
}
