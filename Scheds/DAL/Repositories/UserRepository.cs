using Microsoft.EntityFrameworkCore;
using Scheds.Models.Forum;

namespace Scheds.DAL.Repositories
{
    public class UserRepository
    {
        private readonly SchedsDbContext _context;
        public UserRepository(SchedsDbContext context)
        {
            _context = context;
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Where(user => user.UserId == userId)
                .FirstOrDefaultAsync();
        }
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Where(user => user.UserName == username)
                .FirstOrDefaultAsync();
        }
        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users
                .AnyAsync(user => user.UserId == userId);
        }



    }
}
