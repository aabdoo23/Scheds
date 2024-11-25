using Scheds.Domain.DTOs.Common;
using Scheds.Domain.Entities.Common;
namespace Scheds.Application.Interfaces.Repositories.Common
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> InsertAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<T> UpsertAsync(T entity);
        Task DeleteAsync(string id);
        Task<PaginatedEntityDTO<T>> GetPaginatedContentAsync(IQueryable<T> queryable, int pageNumber, int pageSize);
    }
}
