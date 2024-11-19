using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;

namespace Scheds.Infrastructure.Services
{
    public class EmptyRoomsService(ICourseScheduleRepository courseScheduleRepository) : IEmptyRoomsService
    {
        private readonly ICourseScheduleRepository _courseScheduleRepository = courseScheduleRepository
            ?? throw new ArgumentNullException(nameof(courseScheduleRepository));
        public async Task<List<string>> GetEmptyRooms(string dayOfWeek, string time)
        {
            var allRooms = await _courseScheduleRepository.GetAllRoomsAsync();

            var occupiedRooms = await _courseScheduleRepository.GetOccupiedRoomsAtDayTimeAsync(dayOfWeek, time);

            var emptyRooms = allRooms.Except(occupiedRooms).ToList();
            return emptyRooms;
        }
    }
}
