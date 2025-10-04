using Scheds.Application.Interfaces.Repositories.Common;
using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User> GetOrCreateUserAsync(string email);
    }
}
