using Scheds.Models;

namespace Scheds.DAL.Services
{
    public static class IdGeneration
    {
        public static void GenerateCourseScheduleId(CourseSchedule schedule)
        {
            string schedId = "";
            schedId += schedule.CardId.ToString();
            List<string> days = ["Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday"];
            schedId += days.IndexOf(schedule.DayOfWeek).ToString();
            string startTime = schedule.StartTime.Hours.ToString();
            //schedId += schedule.StartTime.TotalHours.ToString().Substring(0,2);
            if (startTime.Length == 1)
            {
                startTime = "0" + startTime;
            }
            schedId += startTime;
            schedule.ScheduleId = Int32.Parse(schedId);

        }
    }
}
