using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Project.Application.Common.Repository;
using Project.Domain.Model;
using Project.Infastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Infastructure.Service
{
    public class CourseManagementService : GenericService<Course>, ICourseManagementRepository
    {
        private readonly ApplicationDbContext _db;

        public CourseManagementService(IConfiguration configuration, ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }

        public async Task<Course> UpdateAsync(Course model)
        {
            var existing = _db.Course.Local.FirstOrDefault(u => u.CourseId == model.CourseId);
            if (existing != null)
            {
                _db.Entry(existing).State = EntityState.Detached;
            }
            _db.Course.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
