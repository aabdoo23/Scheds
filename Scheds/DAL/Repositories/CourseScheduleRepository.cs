﻿using Microsoft.Data.SqlClient;
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
        public void UpdateCourseScheduleAsync(CardItem course)
        {
            if (course.Schedule == null || course.Schedule.Count == 0)
            {
                return;
            }
            var sqlConnectionString = "Server=db7941.public.databaseasp.net; Database=db7941; User Id=db7941; Password=iQ!9N7b?#X3k; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";
            using var sqlConnection = new SqlConnection(sqlConnectionString);
            sqlConnection.Open();
            var sqlSchedule = @"
                MERGE schedules_Fall25 AS target
                USING (SELECT @cardId AS cardId) AS source
                ON target.cardId = source.cardId
                WHEN MATCHED THEN
                    UPDATE SET 
                        dayOfWeek = @dayOfWeek, 
                        startTime = @startTime, 
                        endTime = @endTime, 
                        location = @location
                WHEN NOT MATCHED THEN
                    INSERT (cardId, dayOfWeek, startTime, endTime, location)
                    VALUES (@cardId, @dayOfWeek, @startTime, @endTime, @location);";


            foreach (var schedule in course.Schedule)
            {
                using var sqlCommandSchedule = new SqlCommand(sqlSchedule, sqlConnection);
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
