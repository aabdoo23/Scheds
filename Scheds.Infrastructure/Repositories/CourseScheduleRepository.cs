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

        ////update or add
        //public void UpdateCourseScheduleAsync(CardItem course)
        //{
        //    if (course.Schedule == null || course.Schedule.Count == 0)
        //    {
        //        return;
        //    }
        //    var sqlConnectionString = "REMOVED";
        //    using var sqlConnection = new SqlConnection(sqlConnectionString);
        //    sqlConnection.Open();
        //    var sqlSchedule = @"
        //        MERGE schedules_Fall25_new AS target
        //        USING (SELECT @scheduleId AS scheduleId) AS source
        //        ON target.scheduleId = source.scheduleId
        //        WHEN MATCHED THEN
        //            UPDATE SET 
        //                cardId=@cardId,
        //                dayOfWeek = @dayOfWeek, 
        //                startTime = @startTime, 
        //                endTime = @endTime, 
        //                location = @location
        //        WHEN NOT MATCHED THEN
        //            INSERT (scheduleId,cardId, dayOfWeek, startTime, endTime, location)
        //            VALUES (@scheduleId,@cardId, @dayOfWeek, @startTime, @endTime, @location);";


        //    foreach (var schedule in course.Schedule)
        //    {
        //        IdGeneration.GenerateCourseScheduleId(schedule);
        //        Console.WriteLine("todb:"+schedule.ToString());
        //        using var sqlCommandSchedule = new SqlCommand(sqlSchedule, sqlConnection);
        //        sqlCommandSchedule.Parameters.AddWithValue("@scheduleId", schedule.ScheduleId);
        //        sqlCommandSchedule.Parameters.AddWithValue("@cardId", course.CardId);
        //        sqlCommandSchedule.Parameters.AddWithValue("@dayOfWeek", schedule.DayOfWeek);
        //        sqlCommandSchedule.Parameters.AddWithValue("@startTime", schedule.StartTime);
        //        sqlCommandSchedule.Parameters.AddWithValue("@endTime", schedule.EndTime);
        //        sqlCommandSchedule.Parameters.AddWithValue("@location", schedule.Location);
        //        sqlCommandSchedule.ExecuteNonQuery();
        //    }
        //}

    }
}
