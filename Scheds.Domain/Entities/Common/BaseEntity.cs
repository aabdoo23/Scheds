using System.ComponentModel.DataAnnotations;

namespace Scheds.Domain.Entities.Common
{
    public abstract class BaseEntity
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
