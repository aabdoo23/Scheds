using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Scheds.Domain.Entities.Common;

namespace Scheds.Domain.Entities
{
    public class CartSeatModeration : BaseEntity
    {
        [Required]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        public string Section { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;
    }
}
