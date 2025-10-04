using Microsoft.AspNetCore.Mvc;
using Scheds.Application.Interfaces.Services;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController(IEmptyRoomsService emptyRoomsService) : Controller
    {
        private readonly IEmptyRoomsService _emptyRoomsService = emptyRoomsService
            ?? throw new ArgumentNullException(nameof(emptyRoomsService));

        [HttpGet]
        public async Task<IActionResult> GetEmptyRooms(string dayOfWeek, string time)
        {
            var emptyRooms = await _emptyRoomsService.GetEmptyRooms(dayOfWeek, time);

            if (emptyRooms.Count == 0)
            {
                return NotFound("No empty rooms available at the specified time.");
            }

            return Ok(emptyRooms);
        }

        [HttpGet("availability")]
        public async Task<IActionResult> GetRoomAvailability(string dayOfWeek, string? time = null, int minimumMinutes = 0)
        {
            TimeSpan? currentTime = null;
            if (!string.IsNullOrEmpty(time) && TimeSpan.TryParse(time, out var parsedTime))
            {
                currentTime = parsedTime;
            }

            var roomAvailability = await _emptyRoomsService.GetRoomAvailabilityForDay(dayOfWeek, currentTime, minimumMinutes);

            if (roomAvailability.Count == 0)
            {
                return NotFound("No rooms available matching the criteria.");
            }

            return Ok(roomAvailability);
        }
    }
}
