using Scheds.Domain.Entities.Common;

namespace Scheds.Domain.Entities
{
    public class CourseSchedule : BaseEntity
    {
        public string CardItemId { get; set; }
        public virtual CardItem CardItem { get; set; } = null!;
        public string DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Location { get; set; }
    }
}
