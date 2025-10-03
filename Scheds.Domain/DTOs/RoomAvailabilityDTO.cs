namespace Scheds.Domain.DTOs
{
    public class RoomAvailabilityDTO
    {
        public string RoomNumber { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public TimeSpan? AvailableFrom { get; set; }
        public TimeSpan? AvailableUntil { get; set; }
        public int ContinuousMinutesAvailable { get; set; }
        public List<TimeBlock> BusyPeriods { get; set; } = new List<TimeBlock>();
        public List<TimeBlock> FreePeriods { get; set; } = new List<TimeBlock>();
    }

    public class TimeBlock
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
    }
}
