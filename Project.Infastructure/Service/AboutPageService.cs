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
    public class AboutPageService:GenericService<AboutPage>, IAboutPageRepository
    {
        private readonly ApplicationDbContext _db;

        public AboutPageService(IConfiguration configuration, ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }

        public async Task<AboutPage> UpdateAsync(AboutPage model)
        {
            var existingId = _db.AboutPage.Local.FirstOrDefault(u => u.AboutPageId == model.AboutPageId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.AboutPage.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
