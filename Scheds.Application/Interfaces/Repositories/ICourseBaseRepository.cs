using Scheds.Application.Interfaces.Repositories.Common;
using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Repositories
{
    public interface ICourseBaseRepository : IBaseRepository<CourseBase>
    {
        public Task<CourseBase> GetCourseBaseByCourseCodeAsync(string courseCode);
        public Task<CourseBase> GetCourseBaseByCourseNameAsync(string courseName);
    }
}
