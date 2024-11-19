using Scheds.Application.Interfaces.Repositories.Common;
using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Repositories
{
    public interface ICourseScheduleRepository : IBaseRepository<CourseSchedule>
    {
        public Task<List<CourseSchedule>> GetCourseSchedulesByCardIdAsync(string cardId);
        public Task<List<string>> GetAllRoomsAsync();
        public Task<List<string>> GetOccupiedRoomsAtDayTimeAsync(string dayOfWeek, string time);
    }
}
