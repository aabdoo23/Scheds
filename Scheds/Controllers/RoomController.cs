using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scheds.DAL;

namespace Scheds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : Controller
    {
        private readonly SchedsDbContext _context;
        public RoomController(SchedsDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetEmptyRooms(string dayOfWeek, string time)
        {
            var t = TimeSpan.Parse(time);
            // Find occupied rooms using deferred execution
            var occupiedRoomsQuery = _context.Schedules_Spr24
                .Where(schedule => schedule.DayOfWeek == dayOfWeek &&
                    t >= schedule.StartTime &&
                    t < schedule.EndTime)
                .Select(schedule => schedule.Location);

            var allRooms = await _context.Schedules_Spr24
            .Select(schedule => schedule.Location)
            .Where(room => !string.IsNullOrWhiteSpace(room) && room != "Online")
            .Distinct()
            .ToListAsync();

            // Fetch occupied rooms
            var occupiedRooms = await occupiedRoomsQuery.ToListAsync();

            // Get the empty rooms
            var emptyRooms = allRooms.Except(occupiedRooms).ToList();


            if (emptyRooms.Count == 0)
            {
                return NotFound("No empty rooms available at the specified time.");
            }

            return Ok(emptyRooms);
        }


    }
}
