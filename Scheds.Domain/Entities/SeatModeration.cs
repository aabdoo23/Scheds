using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Scheds.Domain.Entities.Common;

namespace Scheds.Domain.Entities
{
    public class SeatModeration : BaseEntity
    {
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public DateTime LastUpdated { get; set; }
        public SeatModeration()
        {
        }
        
        public SeatModeration(string courseCodeSection)
        {
            Id = courseCodeSection;
        }
    }
}