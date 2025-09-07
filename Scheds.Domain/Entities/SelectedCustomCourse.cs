using System.ComponentModel.DataAnnotations;

namespace Scheds.Domain.Entities
{
    public class SelectedCustomCourse
    {
        [Key]
        public int Id { get; set; }
        
        public int ScheduleGenerationId { get; set; }
        
        public string CourseCode { get; set; }
        
        public string CourseName { get; set; }
        
        public string CustomMainSection { get; set; }
        
        public string CustomSubSection { get; set; }
        
        public string CustomProfessor { get; set; }
        
        public string CustomTA { get; set; }
        
        // Navigation property
        public virtual ScheduleGeneration ScheduleGeneration { get; set; }
    }
}
