namespace Scheds.Models
{
    public class CourseSchedule
    {
        public int ScheduleId { get; set; }
        public int CardId { get; set; }
        public string DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Location { get; set; }
        public CourseSchedule()
        {
            
        }
        public CourseSchedule(int scheduleId ,int cardId, string dayOfWeek, TimeSpan startTime, TimeSpan endTime, string location)
        {
            ScheduleId = scheduleId;
            CardId = cardId;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            Location = location;
        }
        public override string ToString()
        {
            return $"ScheduleId: {ScheduleId}, CardId: {CardId}, DayOfWeek: {DayOfWeek}, StartTime: {StartTime}, EndTime: {EndTime}, Location: {Location}";
        }
    }
}
