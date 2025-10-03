using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Entities;

namespace Scheds.Infrastructure.Services
{
    public class CourseBaseService(ICourseBaseRepository courseBaseRepository, ICardItemRepository cardItemRepository) : ICourseBaseService
    {
        private readonly ICourseBaseRepository _courseBaseRepository = courseBaseRepository ?? throw new ArgumentNullException(nameof(courseBaseRepository));
        private readonly ICardItemRepository _cardItemRepository = cardItemRepository ?? throw new ArgumentNullException(nameof(cardItemRepository));

        public async Task<List<CourseBase>> GetFilteredCourses(string searchTerm = "")
        {
            var allCourses = await _courseBaseRepository.GetAllAsync();
            var filteredCourses = allCourses
                .Where(c => string.IsNullOrWhiteSpace(searchTerm) ||
                           (!string.IsNullOrWhiteSpace(c.CourseName) && c.CourseName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                           (!string.IsNullOrWhiteSpace(c.CourseCode) && c.CourseCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            return filteredCourses;
        }

        public async Task<List<string>> GetCourseSections(string courseCode)
        {
            if (string.IsNullOrWhiteSpace(courseCode))
            {
                return new List<string>();
            }

            // Get all CardItems (which contain section info) for the specified course code
            var cardItems = await _cardItemRepository.GetCardItemsByCourseCodeAsync(courseCode);
            var courseSections = cardItems
                .Where(item => !string.IsNullOrWhiteSpace(item.Section))
                .Select(item => item.Section)
                .Distinct()
                .OrderBy(section => section)
                .ToList();
                
            return courseSections;
        }
    }
}