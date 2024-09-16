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
        public async Task AddCourseScheduleAsync(CourseSchedule schedule)
        {
            context.Schedules_Fall25.Add(schedule);
            await context.SaveChangesAsync();
        }
        //update or add
        public async Task UpdateCourseScheduleAsync(CourseSchedule schedule)
        {
            CourseSchedule existingSchedule = await context.Schedules_Fall25
                .Where(s => s.CardId == schedule.CardId)
                .FirstOrDefaultAsync();
            if (existingSchedule == null)
            {
                await AddCourseScheduleAsync(schedule);
            }
            else
            {
                existingSchedule.CardId = schedule.CardId;
                existingSchedule.StartTime = schedule.StartTime;
                existingSchedule.EndTime = schedule.EndTime;
                existingSchedule.DayOfWeek = schedule.DayOfWeek;
                existingSchedule.Location = schedule.Location;
                await context.SaveChangesAsync();
            }
        }
    }
}
