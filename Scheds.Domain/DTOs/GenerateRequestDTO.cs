using Scheds.Domain.Entities;

namespace Scheds.Domain.DTOs
{
    public class GenerateRequestDTO
    {
        public List<bool> SelectedDays { get; set; }
        public string DaysStart { get; set; }
        public string DaysEnd { get; set; }
        public int MinimumNumberOfItemsPerDay { get; set; }
        public int LargestAllowedGap { get; set; }
        public int NumberOfDays { get; set; }
        public int MaxNumberOfGeneratedSchedules { get; set; }
        public bool UseLiveData { get; set; }
        public bool IsNumberOfDaysSelected { get; set; }
        public bool IsEngineering { get; set; }
        public List<CourseBase> SelectedItems { get; set; }
        public List<CustomCourseBaseDTO> CustomSelectedItems { get; set; }
    }

}
