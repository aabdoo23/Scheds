using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Scheds.Domain.Entities
{
public class User
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Email { get; set; } = string.Empty;
        public virtual ICollection<SeatModeration> SeatModerations { get; set; } = new List<SeatModeration>();
    
}
}