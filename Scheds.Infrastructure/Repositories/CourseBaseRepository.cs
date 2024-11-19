using Microsoft.EntityFrameworkCore;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Contexts;
using Scheds.Infrastructure.Repositories.Common;

namespace Scheds.Infrastructure.Repositories
{
    public class CourseBaseRepository(SchedsDbContext context) : BaseRepository<CourseBase>(context), ICourseBaseRepository
    {
        private readonly SchedsDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<CourseBase> GetCourseBaseByCourseCodeAsync(string courseCode)
        {
            return await _context.CourseBases
                .Where(courseBase => courseBase.CourseCode == courseCode)
                .FirstOrDefaultAsync();
        }

        public async Task<CourseBase> GetCourseBaseByCourseNameAsync(string courseName)
        {
            return await _context.CourseBases
                .Where(courseBase => courseBase.CourseName == courseName)
                .FirstOrDefaultAsync();
        }

        //        public void UpdateCourseBaseAsync(CardItem course)
        //        {
        //            var sqlConnectionString = "Server=db7941.public.databaseasp.net; Database=db7941; User Id=db7941; Password=iQ!9N7b?#X3k; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";
        //            using var sqlConnection = new SqlConnection(sqlConnectionString);
        //            sqlConnection.Open();
        //            var sqlCourseBase = @"
        //MERGE courseBase_Fall25 AS target
        //USING (SELECT @courseCode AS courseCode) AS source
        //ON target.courseCode = source.courseCode
        //WHEN MATCHED THEN
        //    UPDATE SET 
        //        courseName = @courseName
        //WHEN NOT MATCHED THEN
        //    INSERT (courseCode, courseName)
        //    VALUES (@courseCode, @courseName);";

        //            using var sqlCommandCourseBase = new SqlCommand(sqlCourseBase, sqlConnection);
        //            sqlCommandCourseBase.Parameters.AddWithValue("@courseCode", course.CourseCode);
        //            sqlCommandCourseBase.Parameters.AddWithValue("@courseName", course.CourseName);
        //            Console.WriteLine(sqlCommandCourseBase.ExecuteNonQuery());
        //        }

    }
}
