using Scheds.Domain.Entities;

namespace Scheds.Domain.DTOs
{
    public class ReturnedCardItemDTO
    {
        public string CardId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public string Section { get; set; }
        public double Credits { get; set; }
        public string Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Room { get; set; }
        public string SubType { get; set; }
        public int SeatsLeft { get; set; }

        public ReturnedCardItemDTO(CardItem item)
        {
            CardId = item.Id;
            CourseName = item.CourseName;
            CourseCode = item.CourseCode;
            InstructorName = string.IsNullOrWhiteSpace(item.Instructor) ? "Pending" : item.Instructor;
            Section = item.Section;
            SubType = item.SubType;
            Credits = item.Credits;
            SeatsLeft = item.SeatsLeft;
            if (item.CourseSchedules.Count == 0)
            {
                Day = "";
                StartTime = TimeSpan.Parse("00:00");
                EndTime = TimeSpan.Parse("00:00");
                Room = "";
            }
            else
            {
                Day = item.CourseSchedules.First().DayOfWeek;
                StartTime = item.GetStartTime();
                EndTime = item.GetEndTime();
                Room = item.GetRoom();

            }
        }
    }
}
