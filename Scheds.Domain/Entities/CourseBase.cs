using Scheds.Domain.Entities.Common;

namespace Scheds.Domain.Entities
{
    public class CourseBase : BaseEntity
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
    }
}
