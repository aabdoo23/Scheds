using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Entities;

namespace Scheds.Infrastructure.Services
{
    public class CourseBaseService : ICourseBaseService
    {
        private readonly ICourseBaseRepository _courseBaseRepository;

        public CourseBaseService(ICourseBaseRepository courseBaseRepository)
        {
            _courseBaseRepository = courseBaseRepository ?? throw new ArgumentNullException(nameof(courseBaseRepository));
        }

        public async Task<List<CourseBase>> GetFilteredCourses(string searchTerm = "")
        {
            var allCourses = await _courseBaseRepository.GetAllAsync();
            var filteredCourses = allCourses
                .Where(c => (string.IsNullOrWhiteSpace(searchTerm)) ||
                           (!string.IsNullOrWhiteSpace(c.CourseName) && c.CourseName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                           (!string.IsNullOrWhiteSpace(c.CourseCode) && c.CourseCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            return filteredCourses;
        }
    }
}