using Scheds.Domain.Entities.Common;

namespace Scheds.Domain.DTOs.Common
{
    public class PaginatedEntityDTO<T> where T : BaseEntity
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IList<T> Data { get; set; }
    }
}
