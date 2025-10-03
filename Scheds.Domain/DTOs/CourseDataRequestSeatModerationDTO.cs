namespace Scheds.Domain.DTOs
{
    public class CourseDataRequestSeatModerationDTO
    {
        public List<string> CourseCode { get; set; } = new();
        public List<string> Sections { get; set; } = new();
    }
}