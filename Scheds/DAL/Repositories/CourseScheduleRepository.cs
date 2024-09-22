using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Scheds.DAL.Services;
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
            return await context.Schedules_Fall25_New.ToListAsync();
        }
        public async Task<List<CourseSchedule>> GetCourseSchedulesByCardIdAsync(int cardId)
        {
            return await context.Schedules_Fall25_New
                .Where(schedule => schedule.CardId == cardId)
                .ToListAsync();
        }
        public async Task AddCourseScheduleAsync(CourseSchedule schedule)
        {
            context.Schedules_Fall25_New.Add(schedule);
            await context.SaveChangesAsync();
        }
        //update or add
        public void UpdateCourseScheduleAsync(CardItem course)
        {
            if (course.Schedule == null || course.Schedule.Count == 0)
            {
                return;
            }
            var sqlConnectionString = "REMOVED";
            using var sqlConnection = new SqlConnection(sqlConnectionString);
            sqlConnection.Open();
            var sqlSchedule = @"
                MERGE schedules_Fall25_new AS target
                USING (SELECT @scheduleId AS scheduleId) AS source
                ON target.scheduleId = source.scheduleId
                WHEN MATCHED THEN
                    UPDATE SET 
                        cardId=@cardId,
                        dayOfWeek = @dayOfWeek, 
                        startTime = @startTime, 
                        endTime = @endTime, 
                        location = @location
                WHEN NOT MATCHED THEN
                    INSERT (scheduleId,cardId, dayOfWeek, startTime, endTime, location)
                    VALUES (@scheduleId,@cardId, @dayOfWeek, @startTime, @endTime, @location);";


            foreach (var schedule in course.Schedule)
            {
                IdGeneration.GenerateCourseScheduleId(schedule);
                Console.WriteLine("todb:"+schedule.ToString());
                using var sqlCommandSchedule = new SqlCommand(sqlSchedule, sqlConnection);
                sqlCommandSchedule.Parameters.AddWithValue("@scheduleId", schedule.ScheduleId);
                sqlCommandSchedule.Parameters.AddWithValue("@cardId", course.CardId);
                sqlCommandSchedule.Parameters.AddWithValue("@dayOfWeek", schedule.DayOfWeek);
                sqlCommandSchedule.Parameters.AddWithValue("@startTime", schedule.StartTime);
                sqlCommandSchedule.Parameters.AddWithValue("@endTime", schedule.EndTime);
                sqlCommandSchedule.Parameters.AddWithValue("@location", schedule.Location);
                sqlCommandSchedule.ExecuteNonQuery();
            }
        }

    }
}
