using System.ComponentModel.DataAnnotations;

namespace Scheds.Domain.Entities
{
    public class SelectedCourse
    {
        [Key]
        public int Id { get; set; }
        
        public int ScheduleGenerationId { get; set; }
        
        public string CourseCode { get; set; }
        
        public string CourseName { get; set; }
        
        // Navigation property
        public virtual ScheduleGeneration ScheduleGeneration { get; set; }
    }
}
