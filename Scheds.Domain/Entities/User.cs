using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Scheds.Domain.Entities.Common;

namespace Scheds.Domain.Entities
{
    public class User : BaseEntity
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        public virtual ICollection<SeatModeration> SeatModerations { get; set; } = new List<SeatModeration>();
    }
}