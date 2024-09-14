using System.Text.Json.Serialization;

namespace Scheds.Model
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
