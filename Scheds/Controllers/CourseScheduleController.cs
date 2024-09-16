using Microsoft.AspNetCore.Mvc;
using Scheds.DAL.Repositories;
using Scheds.Models;

namespace Scheds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseScheduleController : Controller
    {
        private readonly CourseScheduleRepository courseScheduleRepository;
        public CourseScheduleController(CourseScheduleRepository courseScheduleRepository)
        {
            this.courseScheduleRepository = courseScheduleRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetCourseSchedulesByCardId(int cardId)
        {
            var schedules = await courseScheduleRepository.GetCourseSchedulesByCardIdAsync(cardId);
            return Ok(schedules);
        }
        [HttpPost]
        public async Task<IActionResult> AddCourseSchedule(CourseSchedule schedule)
        {
            await courseScheduleRepository.AddCourseScheduleAsync(schedule);
            return Ok();
        }
    }
}
