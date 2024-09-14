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
            return await _context.CourseBase_Spr24.ToListAsync();
        }

        public async Task<CourseBase> GetCourseBaseByCourseCodeAsync(string courseCode)
        {
            return await _context.CourseBase_Spr24
                .Where(courseBase => courseBase.CourseCode == courseCode)
                .FirstOrDefaultAsync();
        }

        public async Task<CourseBase> GetCourseBaseByNameAsync(string courseName)
        {
            return await _context.CourseBase_Spr24
                .Where(courseBase => courseBase.CourseName == courseName)
                .FirstOrDefaultAsync();
        }
    }
}
