using Microsoft.Extensions.Configuration;
using Project.Application.Common.Repository;
using Project.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infastructure.Service
{
    public class CategoryService:GenericService<Category>,ICategoryRepository
    {
        private readonly Data.ApplicationDbContext _db;

        public CategoryService(IConfiguration configuration, Data.ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }

        public async Task<Category> UpdateAsync(Category model)
        {
            var existingId = _db.Category.Local.FirstOrDefault(u => u.CategoryId == model.CategoryId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            _db.Category.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
