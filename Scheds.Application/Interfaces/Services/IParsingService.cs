using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Services
{
    public interface IParsingService
    {
        public Task<List<CardItem>> ParseSelfServiceResponse(string response);
    }
}
