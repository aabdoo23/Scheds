namespace Scheds.Models
{
    public class GenerateRequest
    {
        public List<CourseBase> selectedItems { get; set; }
        public List<CustomCourseBase> customSelectedItems { get; set; }
        public int minimumNumberOfItemsPerDay { get; set; }
        public int largestAllowedGap { get; set; }
        public int numberOfDays { get; set; }
        public bool specificDays { get; set; }
        public List<string> selectedDays { get; set; }
        public string daysStart { get; set; }
        public string daysEnd { get; set; }
        public int maxNumberOfGeneratedSchedules { get; set;}
        public GenerateRequest()
        {
            selectedItems = new List<CourseBase>();
            customSelectedItems = new List<CustomCourseBase>();
            selectedDays = new List<string>();
        }
    }
}
