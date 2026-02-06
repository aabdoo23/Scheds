namespace Scheds.Domain.DTOs
{
    public class CardItemSummaryDTO
    {
        public string CourseCode { get; set; }
        public string Instructor { get; set; }
        public string Section { get; set; }
        public string SubType { get; set; }
        public string? ScheduleDisplay { get; set; }
    }
}
