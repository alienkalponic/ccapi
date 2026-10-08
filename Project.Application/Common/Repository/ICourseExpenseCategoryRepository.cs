using Project.Domain.Model;
using System.Threading.Tasks;

namespace Project.Application.Common.Repository
{
    public interface ICourseExpenseCategoryRepository : IGenericRepository<CourseExpenseCategory>
    {
        Task<CourseExpenseCategory> UpdateAsync(CourseExpenseCategory model);
    }
}
