using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scheds.Domain.Entities
{
    public class CartSeatModeration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        public string Section { get; set; } = string.Empty;

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
