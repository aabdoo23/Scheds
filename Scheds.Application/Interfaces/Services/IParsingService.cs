using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Services
{
    public interface IParsingService
    {
        public Task<List<CardItem>> ParseSelfServiceResponse(string response);

        /// <summary>
        /// Number of sections in a Self-Service response, without building cards or touching the
        /// database. Lets a caller drive pagination during a parallel download phase.
        /// </summary>
        public int CountSections(string response);
    }
}
