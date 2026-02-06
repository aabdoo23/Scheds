using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Services
{
    public interface ICardItemService
    {
        Task LiveFetchUpsertAsync(CardItem cardItem);
        Task LiveFetchRefreshCourseAsync(string courseCode, List<CardItem> cards);
    }
}
