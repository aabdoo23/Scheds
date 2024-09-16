namespace Scheds.Models
{
    public class GenerateRequest
    {
        public List<bool> selectedDays { get; set; }
        public string daysStart { get; set; }
        public string daysEnd { get; set; }
        public int minimumNumberOfItemsPerDay { get; set; }
        public int largestAllowedGap { get; set; }
        public int numberOfDays { get; set; }
        public int maxNumberOfGeneratedSchedules { get; set; }
        public bool useLiveData { get; set; }
        public bool isNumberOfDaysSelected { get; set; }
        public List<CourseBase> selectedItems { get; set; }
        public List<CustomCourseBase> customSelectedItems { get; set; }
    }

}
