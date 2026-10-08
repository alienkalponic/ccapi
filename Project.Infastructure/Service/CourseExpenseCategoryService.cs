using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Project.Application.Common.Repository;
using Project.Domain.Model;
using Project.Infastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Infastructure.Service
{
    public class CourseExpenseCategoryService : GenericService<CourseExpenseCategory>, ICourseExpenseCategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CourseExpenseCategoryService(IConfiguration configuration, ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }

        public async Task<CourseExpenseCategory> UpdateAsync(CourseExpenseCategory model)
        {
            var existingId = _db.CourseExpenseCategory.Local.FirstOrDefault(u => u.CourseExpenseCategoryId == model.CourseExpenseCategoryId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.CourseExpenseCategory.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
