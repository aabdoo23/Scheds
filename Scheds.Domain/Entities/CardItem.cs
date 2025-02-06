using Scheds.Domain.Entities.Common;

namespace Scheds.Domain.Entities
{
    public class CardItem : BaseEntity
    {
        public CardItem()
        {
            CourseSchedules = new List<CourseSchedule>();
        }

        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Instructor { get; set; }
        public double Credits { get; set; }
        public string Section { get; set; }
        public int SeatsLeft { get; set; }
        public string SubType { get; set; }
        public virtual ICollection<CourseSchedule> CourseSchedules { get; set; }
        public DateTime LastUpdate { get; set; }

        public bool IsMainSection()
        {
            return Section.Length == 2;
        }

        public static CardItem CopyCardItem(CardItem item)
        {
            var copy = new CardItem
            {
                Id = item.Id,
                CourseName = item.CourseName,
                CourseCode = item.CourseCode,
                Credits = item.Credits,
                Instructor = item.Instructor,
                Section = item.Section,
                SubType = item.SubType,
                SeatsLeft = item.SeatsLeft,
                LastUpdate = item.LastUpdate
            };

            // Create new CourseSchedules with new IDs
            foreach (var schedule in item.CourseSchedules)
            {
                copy.CourseSchedules.Add(schedule);
            }

            return copy;
        }

        public TimeSpan GetStartTime()
        {
            return CourseSchedules.First().StartTime; // Index 0 corresponds to start time
        }
        public TimeSpan GetEndTime()
        {
            return CourseSchedules.First().EndTime; // Index 2 corresponds to end time
        }
        public string GetRoom() { return CourseSchedules.First().Location; }

        public bool ConflictsWith(CardItem other)
        {
            foreach (var thisSchedule in this.CourseSchedules)
            {
                foreach (var otherSchedule in other.CourseSchedules)
                {
                    if (ScheduleConflict(thisSchedule, otherSchedule))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HasMultipleSchedules()
        {
            return CourseSchedules.Count > 1;
        }

        private static bool ScheduleConflict(CourseSchedule schedule1, CourseSchedule schedule2)
        {

            string day1 = schedule1.DayOfWeek;
            TimeSpan startTime1 = schedule1.StartTime;
            TimeSpan endTime1 = schedule1.EndTime;

            string day2 = schedule2.DayOfWeek;
            TimeSpan startTime2 = schedule2.StartTime;
            TimeSpan endTime2 = schedule2.EndTime;

            // Check if the days are the same
            if (day1 != day2)
            {
                return false; // Different days, no conflict
            }
            int startHr1, endHr1, startMin1, endMin1;
            startHr1 = startTime1.Hours;
            startMin1 = startTime1.Minutes;
            endHr1 = endTime1.Hours;
            endMin1 = endTime1.Minutes;

            int startHr2, endHr2, startMin2, endMin2;
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
        public override string ToString()
        {
            foreach (var s in CourseSchedules) Console.WriteLine(s.ToString());
            return $"CardItemId: {Id}, CourseCode: {CourseCode}, CourseName: {CourseName}, Instructor: {Instructor}, Credits: {Credits}, Section: {Section}, SeatsLeft: {SeatsLeft}, SubType: {SubType}, LastUpdate: {LastUpdate}";
        }
    }
}
