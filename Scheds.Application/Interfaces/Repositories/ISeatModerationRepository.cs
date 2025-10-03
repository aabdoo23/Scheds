using Scheds.Application.Interfaces.Repositories.Common;
using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Repositories
{
    public interface ISeatModerationRepository : IBaseRepository<SeatModeration>
    {
        Task<SeatModeration?> GetByIdWithUsersAsync(string id);
        Task<List<SeatModeration>> GetAllWithUsersAsync();
        Task<List<SeatModeration>> GetUserSeatModerationsAsync(string userEmail);
    }
}
