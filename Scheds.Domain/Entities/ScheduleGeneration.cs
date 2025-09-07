using System.ComponentModel.DataAnnotations;

namespace Scheds.Domain.Entities
{
    public class ScheduleGeneration
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime GeneratedAt { get; set; }
        
        public int NumberOfSchedulesGenerated { get; set; }
        
        public bool UsedLiveData { get; set; }
        
        public bool ConsideredZeroSeats { get; set; }
        
        public bool IsEngineering { get; set; }
        
        public int MinimumNumberOfItemsPerDay { get; set; }
        
        public int LargestAllowedGap { get; set; }
        
        public int NumberOfDays { get; set; }
        
        public bool IsNumberOfDaysSelected { get; set; }
        
        public string DaysStart { get; set; }
        
        public string DaysEnd { get; set; }
        
        public string SelectedDaysJson { get; set; } // JSON representation of selected days
        
        public string ClientIpAddress { get; set; }
        
        public string UserAgent { get; set; }
        
        // Navigation properties
        public virtual ICollection<SelectedCourse> SelectedCourses { get; set; }
        public virtual ICollection<SelectedCustomCourse> SelectedCustomCourses { get; set; }
    }
}
