using Microsoft.AspNetCore.Mvc;
using Scheds.DAL.Repositories;
using Scheds.Models;

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
            var courses = await courseBaseRepository.GetAllCourseBasesAsync();
            foreach(CourseBase course in courses) Console.WriteLine(course.CourseCode);
            return Ok(courses);
        }
    }

}
