using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Project.Application.Common.Repository;
using Project.Domain.Model;
using Project.Infastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infastructure.Service
{
    public class CourseAccountantService:GenericService<CourseAccountant>, ICourseAccountantRepository
    {
        private readonly ApplicationDbContext _db;
        public CourseAccountantService(IConfiguration configuration, ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }

        public async Task<CourseAccountant> UpdateAsync(CourseAccountant model)
        {
            var existingId = _db.CourseAccountant.Local.FirstOrDefault(u => u.CourseAccountantId == model.CourseAccountantId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.CourseAccountant.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
