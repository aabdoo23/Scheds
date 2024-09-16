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
        private readonly NuDealer nuDealer;

        public CourseBaseController(CourseBaseRepository courseBaseRepository, NuDealer nuDealer)
        {
            this.courseBaseRepository = courseBaseRepository;
            this.nuDealer = nuDealer;
        }

        [HttpGet("getAllCourses")]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await courseBaseRepository.GetAllCourseBasesAsync();
            // foreach(CourseBase course in courses) Console.WriteLine(course.CourseCode);
            return Ok(courses);
        }
        [HttpGet("search/{query}")]
        public async Task<IActionResult> SearchCourses(string query)
        {
            var courses = await nuDealer.FetchCoursBases(query);
            return Ok(courses);
        }
        [HttpGet("getCourseBaseByCourseCode")]
        public async Task<IActionResult> GetCourseBaseByCourseCode(string courseCode)
        {
            var course = await courseBaseRepository.GetCourseBaseByCourseCodeAsync(courseCode);
            return Ok(course);
        }
        [HttpGet("getCourseBaseByName")]
        public async Task<IActionResult> GetCourseBaseByName(string courseName)
        {
            var course = await courseBaseRepository.GetCourseBaseByNameAsync(courseName);
            return Ok(course);
        }
        [HttpPost("addCourseBase")]
        public async Task<IActionResult> AddCourseBase(CourseBase courseBase)
        {
            await courseBaseRepository.AddCourseBaseAsync(courseBase);
            return Ok();
        }

    }

}
