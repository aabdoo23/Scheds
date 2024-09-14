using System.Text.Json.Serialization;

namespace Scheds.Models
{
    public class CourseBase
    {
        public string CourseCode {  get; set; }
        public string CourseName { get; set; }
        public CourseBase(string courseCode, string courseName)
        {
            CourseCode = courseCode;
            CourseName = courseName;
        }
    }
}
