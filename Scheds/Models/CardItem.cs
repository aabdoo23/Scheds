namespace Scheds.Models
{
    public class CardItem
    {
        public int CardId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Instructor { get; set; }
        public decimal Credits { get; set; }
        public string Section { get; set; }
        public int SeatsLeft { get; set; }
        public string SubType { get; set; }
        public List<CourseSchedule> Schedule { get; set; }
        public DateTime LastUpdate { get; set; }

        public bool isMainSection()
        {
            return Section.Length == 2;
        }

        public CardItem(CardItem item)
        {
            this.CardId = item.CardId;
            this.CourseName = item.CourseName;
            this.CourseCode = item.CourseCode;
            this.Credits = item.Credits;
            this.Instructor = item.Instructor;
            this.Section = item.Section;
            this.SubType = item.SubType;
            this.Schedule = item.Schedule;
            this.SeatsLeft = item.SeatsLeft;
            this.LastUpdate = item.LastUpdate;
        }
        public CardItem(int cardId, string courseCode, string courseName, string instructor, decimal credits, string section, int seatsLeft, string subType, List<CourseSchedule> schedule, DateTime lastUpdated)
        {
            this.CardId = cardId;
            this.CourseCode = courseCode;
            this.CourseName = courseName;
            this.Credits = credits;
            this.Instructor = instructor;
            this.Section = section;
            this.SeatsLeft = seatsLeft;
            this.SubType = subType;
            this.Schedule = schedule;
            this.LastUpdate = lastUpdated;
        }
        public CardItem() { }

        public TimeSpan getStartTime()
        {
            return Schedule[0].StartTime; // Index 0 corresponds to start time
        }
        public TimeSpan getEndTime()
        {
            return Schedule[0].EndTime; // Index 2 corresponds to end time
        }
        public string getRoom() { return Schedule[0].Location; }

        public bool conflictsWith(CardItem other)
        {
            foreach (var thisSchedule in this.Schedule)
            {
                foreach (var otherSchedule in other.Schedule)
                {
                    if (scheduleConflict(thisSchedule, otherSchedule))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool hasMultipleSchedules()
        {
            return Schedule.Count > 1;
        }

        private static bool scheduleConflict(CourseSchedule schedule1, CourseSchedule schedule2)
        {

            string day1 = schedule1.DayOfWeek;
            TimeSpan startTime1 = schedule1.StartTime;
            TimeSpan endTime1 = schedule1.EndTime;
            
            string day2 = schedule2.DayOfWeek;
            TimeSpan startTime2 = schedule2.StartTime;
            TimeSpan endTime2 = schedule2.EndTime;

            // Check if the days are the same
            if (day1!=day2)
            {
                return false; // Different days, no conflict
            }
            int startHr1, endHr1, startMin1, endMin1;
            string startAMPM1, endAMPM1;
            startHr1 = startTime1.Hours;
            startMin1 = startTime1.Minutes;
            endHr1 = endTime1.Hours;
            endMin1 = endTime1.Minutes;

            int startHr2, endHr2, startMin2, endMin2;
            string startAMPM2, endAMPM2;
            startHr2 = startTime2.Hours;
            startMin2 = startTime2.Minutes;
            endHr2 = endTime2.Hours;
            endMin2 = endTime2.Minutes;

            bool start1BeforeEnd2 = false, end1AfterStart2 = false;
            if (startHr1 < endHr2) start1BeforeEnd2 = true;
            if (startHr1 == endHr2 && startMin1 <= endMin2) start1BeforeEnd2 = true;
            if (endHr1 > startHr2) end1AfterStart2 = true;
            if (endHr1 == startHr2 && endMin1 > startMin2) end1AfterStart2 = true;


            return start1BeforeEnd2 && end1AfterStart2;

        }
    }
}
