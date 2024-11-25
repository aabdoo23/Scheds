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
    }
}
