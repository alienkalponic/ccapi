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
    public class BannerService:GenericService<Banner>,IBannerRepository
    {
        private readonly ApplicationDbContext _db;
        public BannerService(IConfiguration configuration, ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }
        public async Task<Banner> UpdateAsync(Banner model)
        {
            var existingId = _db.Banner.Local.FirstOrDefault(u => u.BannerId == model.BannerId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.Banner.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
