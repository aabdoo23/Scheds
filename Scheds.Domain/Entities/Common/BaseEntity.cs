using System.ComponentModel.DataAnnotations;

namespace Scheds.Domain.Entities.Common
{
    public abstract class BaseEntity
    {
        public string Id { get; set; }
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
