
using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Services
{
    public interface ICourseBaseService
    {
        public Task<List<CourseBase>> GetFilteredCourses(string searchTerm = "");
    }
}
