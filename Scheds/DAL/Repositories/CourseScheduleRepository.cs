using Microsoft.EntityFrameworkCore;
using Scheds.Models;

namespace Scheds.DAL.Repositories
{
    public class CourseScheduleRepository
    {
        private readonly SchedsDbContext context;
        public CourseScheduleRepository(SchedsDbContext context)
        {
            this.context = context;
        }
        public async Task<List<CourseSchedule>> GetCourseSchedulesAsync()
        {
            return await context.Schedules_Fall25.ToListAsync();
        }
        public async Task<List<CourseSchedule>> GetCourseSchedulesByCardIdAsync(int cardId)
        {
            return await context.Schedules_Fall25
                .Where(schedule => schedule.CardId == cardId)
                .ToListAsync();
        }
    }
}
