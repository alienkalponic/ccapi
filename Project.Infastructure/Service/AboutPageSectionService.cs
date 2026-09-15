using Microsoft.EntityFrameworkCore;
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
    public class AboutPageSectionService:GenericService<AboutPageSection>, IAboutPageSectionRepository
    {
        private readonly Data.ApplicationDbContext _db;
        public AboutPageSectionService(IConfiguration configuration, Data.ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }
        public async Task<AboutPageSection> UpdateAsync(AboutPageSection model)
        {
            var existingId = _db.AboutPageSection.Local.FirstOrDefault(u => u.SectionId == model.SectionId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.AboutPageSection.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
