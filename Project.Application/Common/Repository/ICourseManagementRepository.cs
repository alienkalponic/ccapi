using Project.Domain.Model;
using System.Threading.Tasks;

namespace Project.Application.Common.Repository
{
    public interface ICourseManagementRepository : IGenericRepository<Course>
    {
        Task<Course> UpdateAsync(Course model);
    }
}
