using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace Scheds.Domain.Entities
{
    public class SeatModeration
    {
        [Key] 
        public string CourseCode_Section { get; set; } = string.Empty;
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}