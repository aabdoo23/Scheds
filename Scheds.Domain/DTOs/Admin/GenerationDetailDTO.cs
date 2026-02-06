namespace Scheds.Domain.DTOs.Admin
{
    public class GenerationDetailDTO
    {
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
        public string DaysStart { get; set; } = "";
        public string DaysEnd { get; set; } = "";
        public string? SelectedDaysJson { get; set; }
        public string? ClientIpAddress { get; set; }
        public string? UserAgent { get; set; }
        public List<SelectedCourseDTO> SelectedCourses { get; set; } = [];
        public List<SelectedCustomCourseDTO> SelectedCustomCourses { get; set; } = [];
    }

    public class SelectedCourseDTO
    {
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
    }

    public class SelectedCustomCourseDTO
    {
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string? CustomMainSection { get; set; }
        public string? CustomSubSection { get; set; }
        public string? CustomProfessor { get; set; }
        public string? CustomTA { get; set; }
    }
}
