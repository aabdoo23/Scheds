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
        public async Task UpdateCourseBaseAsync(CourseBase courseBase)
        {
            var existingCourseBase = await _context.CourseBase_Fall25
                .Where(c => c.CourseCode == courseBase.CourseCode)
                .FirstOrDefaultAsync();
            if (existingCourseBase == null)
            {
                await AddCourseBaseAsync(courseBase);
            }
            else
            {
                existingCourseBase.CourseName = courseBase.CourseName;
                await _context.SaveChangesAsync();
            }
        }
    }
}
