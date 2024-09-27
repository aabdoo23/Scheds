using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Scheds.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scheds.DAL.Repositories
{
    public class CourseBaseRepository
    {
        private readonly SchedsDbContext _context;

        public CourseBaseRepository(SchedsDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseBase>> GetAllCourseBasesAsync()
        {
            return await _context.CourseBase_Fall25.ToListAsync();
        }

        public async Task<CourseBase> GetCourseBaseByCourseCodeAsync(string courseCode)
        {
            return await _context.CourseBase_Fall25
                .Where(courseBase => courseBase.CourseCode == courseCode)
                .FirstOrDefaultAsync();
        }

        public async Task<CourseBase> GetCourseBaseByNameAsync(string courseName)
        {
            return await _context.CourseBase_Fall25
                .Where(courseBase => courseBase.CourseName == courseName)
                .FirstOrDefaultAsync();
        }
        public async Task AddCourseBaseAsync(CourseBase courseBase)
        {
            _context.CourseBase_Fall25.Add(courseBase);
            await _context.SaveChangesAsync();
        }
        public void UpdateCourseBaseAsync(CardItem course)
        {
            var sqlConnectionString = "Server=db7941.public.databaseasp.net; Database=db7941; User Id=db7941; Password=iQ!9N7b?#X3k; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";
            using var sqlConnection = new SqlConnection(sqlConnectionString);
            sqlConnection.Open();
            var sqlCourseBase = @"
                MERGE courseBase_Fall25 AS target
                USING (SELECT @courseCode AS courseCode) AS source
                ON target.courseCode = source.courseCode
                WHEN MATCHED THEN
                    UPDATE SET 
                        courseName = @courseName
                WHEN NOT MATCHED THEN
                    INSERT (courseCode, courseName)
                    VALUES (@courseCode, @courseName);";

            using var sqlCommandCourseBase = new SqlCommand(sqlCourseBase, sqlConnection);
            sqlCommandCourseBase.Parameters.AddWithValue("@courseCode", course.CourseCode);
            sqlCommandCourseBase.Parameters.AddWithValue("@courseName", course.CourseName);
            Console.WriteLine( sqlCommandCourseBase.ExecuteNonQuery());
        }

    }
}
