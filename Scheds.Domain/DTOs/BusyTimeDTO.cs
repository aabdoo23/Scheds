namespace Scheds.Domain.DTOs
{
    /// <summary>
    /// A block of time the student is unavailable (job, commute, prayer, training…).
    /// The generator treats it like a fixed phantom class: no course meeting may
    /// overlap it. Day uses the same index as SelectedDays (0=Sat … 5=Thu).
    /// </summary>
    public class BusyTimeDTO
    {
        public int Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
