using Scheds.Domain.DTOs;

namespace Scheds.Application.Interfaces.Services
{
    public interface IEmptyRoomsService
    {
        public Task<List<string>> GetEmptyRooms(string dayOfWeek, string time);
        public Task<List<RoomAvailabilityDTO>> GetRoomAvailabilityForDay(string dayOfWeek, TimeSpan? currentTime = null, int minimumMinutes = 0);
    }
}
