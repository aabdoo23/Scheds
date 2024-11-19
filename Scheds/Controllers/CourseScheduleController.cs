using Microsoft.AspNetCore.Mvc;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseScheduleController(ICourseScheduleRepository courseScheduleRepository) : Controller
    {
        private readonly ICourseScheduleRepository courseScheduleRepository = courseScheduleRepository
            ?? throw new ArgumentNullException(nameof(courseScheduleRepository));

        [HttpGet]
        public async Task<IActionResult> GetCourseSchedulesByCardId(string cardId)
        {
            var schedules = await courseScheduleRepository.GetCourseSchedulesByCardIdAsync(cardId);
            return Ok(schedules);
        }

        [HttpPost]
        public async Task<IActionResult> AddCourseSchedule(CourseSchedule schedule)
        {
            await courseScheduleRepository.InsertAsync(schedule);
            return Ok();
        }
    }
}
