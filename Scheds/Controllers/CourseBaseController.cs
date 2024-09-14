using Microsoft.AspNetCore.Mvc;
using Scheds.DAL.Repositories;

namespace Scheds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseBaseController : Controller
    {
        private readonly CourseBaseRepository courseBaseRepository;

        public CourseBaseController(CourseBaseRepository courseBaseRepository)
        {
            this.courseBaseRepository = courseBaseRepository;
        }

        [HttpGet("getAllCourses")]
        public async Task<IActionResult> GetAllCourses()
        {
            Console.WriteLine("here");
            var courses = await courseBaseRepository.GetAllCourseBasesAsync();
            return Ok(courses);
        }
    }

}
