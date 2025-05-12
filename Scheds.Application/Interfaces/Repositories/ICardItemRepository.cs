using Scheds.Application.Interfaces.Repositories.Common;
using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Repositories
{
    public interface ICardItemRepository : IBaseRepository<CardItem>
    {
        public Task<List<CardItem>> GetCardItemsByCourseCodeAsync(string courseCode);
        public Task<List<CardItem>> Search(string query);
    }
}
