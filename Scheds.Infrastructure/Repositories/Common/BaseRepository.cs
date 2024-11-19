using Microsoft.EntityFrameworkCore;
using Scheds.Application.Interfaces.Repositories.Common;
using Scheds.Domain.DTOs.Common;
using Scheds.Domain.Entities.Common;
using Scheds.Infrastructure.Contexts;

namespace Scheds.Infrastructure.Repositories.Common
{
    public class BaseRepository<T>(SchedsDbContext context) : IBaseRepository<T> where T : BaseEntity
    {
        private readonly SchedsDbContext _context = context 
            ?? throw new ArgumentNullException(nameof(context));
        private readonly DbSet<T> _dbSet = context.Set<T>();

        public async Task<T> InsertAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _dbSet.FindAsync(id);
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpsertAsync(T entity)
        {
            var existingEntity = await _dbSet.FindAsync(entity.Id);

            if (existingEntity == null)
            {
                return await InsertAsync(entity);
            }

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existingEntity;
        }

        public virtual async Task<PaginatedEntityDTO<T>> GetPaginatedContentAsync(IQueryable<T> queryable, int pageNumber, int pageSize)
        {
            var totalCount = await queryable.CountAsync();

            var data = await queryable
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedEntityDTO<T>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }
    }
}
