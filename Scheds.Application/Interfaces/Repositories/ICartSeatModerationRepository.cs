using Scheds.Application.Interfaces.Repositories.Common;
using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Repositories
{
    public interface ICartSeatModerationRepository : IBaseRepository<CartSeatModeration>
    {
        Task<CartSeatModeration?> GetByUserAndCourseAsync(string userId, string courseCode, string section);
        Task<List<CartSeatModeration>> GetUserCartItemsAsync(string userId);
        Task ClearUserCartAsync(string userId);
    }
}
