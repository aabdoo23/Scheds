namespace Scheds.Domain.DTOs
{
    public class RemoveFromSeatModerationCartRequest
    {
        public string CourseCode { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
    }
}