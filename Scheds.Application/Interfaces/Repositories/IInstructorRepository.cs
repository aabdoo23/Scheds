using Scheds.Application.Interfaces.Repositories.Common;
using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Repositories
{
    public interface IInstructorRepository : IBaseRepository<Instructor>
    {
        public Task<string?> GetInstructorNameById(string id);
    }
}
