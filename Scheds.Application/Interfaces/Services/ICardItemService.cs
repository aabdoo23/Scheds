using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Services
{
    public interface ICardItemService
    {
        public Task LiveFetchUpsertAsync(CardItem cardItem);
    }
}
