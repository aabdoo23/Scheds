namespace Scheds.Models
{
    public class CourseSchedule
    {
        public int CardId { get; set; }
        public string DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Location { get; set; }
        public CourseSchedule()
        {
            
        }
        public CourseSchedule(int cardId, string dayOfWeek, TimeSpan startTime, TimeSpan endTime, string location)
        {
            CardId = cardId;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            Location = location;
        }
    }
}
