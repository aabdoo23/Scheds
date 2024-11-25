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
    }
}
