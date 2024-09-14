namespace Scheds.Models
{
    public class ReturnedCardItem
    {
        public int cardId { get; set; }
        public string courseCode { get; set; }
        public string courseName { get; set; }
        public string instructorName { get; set; }
        public string section { get; set; }
        public decimal credits { get; set; }
        public string day { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public string room { get; set; }
        public string subType { get; set; }
        public int seatsLeft { get; set; }

        public ReturnedCardItem(CardItem item)
        {
            this.cardId = item.CardId;
            this.courseName = item.CourseName;
            this.courseCode = item.CourseCode;
            this.instructorName = item.Instructor;
            this.section = item.Section;
            this.subType = item.SubType;
            this.credits = item.Credits;
            this.seatsLeft = item.SeatsLeft;
            if (item.Schedule.Count==0)
            {
                this.day = "";
                this.startTime = TimeSpan.Parse("00:00");
                this.endTime = TimeSpan.Parse("00:00");
                this.room = "";
            }
            else
            {
                this.day = item.Schedule[0].DayOfWeek;
                this.startTime = item.getStartTime();
                this.endTime = item.getEndTime();
                this.room = item.getRoom();

            }
        }

        public ReturnedCardItem()
        {
            
        }
        public ReturnedCardItem(int cardId, string courseCode, string courseName, string instructorName, string section, decimal credits, string day, TimeSpan startTime, TimeSpan endTime, string room, string subType, int seatsLeft)
        {
            this.cardId = cardId;
            this.courseCode = courseCode;
            this.courseName = courseName;
            this.instructorName = instructorName;
            this.section = section;
            this.credits = credits;
            this.day = day;
            this.startTime = startTime;
            this.endTime = endTime;
            this.room = room;
            this.subType = subType;
            this.seatsLeft = seatsLeft;
        }
    }
}
