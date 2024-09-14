using Microsoft.EntityFrameworkCore;
using Scheds.Model;

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
            return await context.Schedules_Spr24.ToListAsync();
        }
        public async Task<List<CourseSchedule>> GetCourseSchedulesByCardIdAsync(int cardId)
        {
            return await context.Schedules_Spr24
                .Where(schedule => schedule.CardId == cardId)
                .ToListAsync();
        }
    }
}
