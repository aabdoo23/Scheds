using Microsoft.EntityFrameworkCore;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Contexts;
using Scheds.Infrastructure.Repositories.Common;

namespace Scheds.Infrastructure.Repositories
{
    public class UserRepository(SchedsDbContext context) 
        : BaseRepository<User>(context), IUserRepository
    {
        private readonly SchedsDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetOrCreateUserAsync(string email)
        {
            var user = await GetByEmailAsync(email);
            
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = email
                };
                
                user = await InsertAsync(user);
            }

            return user;
        }
    }
}
