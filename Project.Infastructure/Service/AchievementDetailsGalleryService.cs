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
    public class AchievementDetailsGalleryService:GenericService<AchievementDetailsGallery>, IAchievementDetailsGalleryRepository
    {
        private readonly ApplicationDbContext _db;
        public AchievementDetailsGalleryService(IConfiguration configuration, ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }
        public async Task<AchievementDetailsGallery> UpdateAsync(AchievementDetailsGallery model)
        {
            var existingId = _db.AchievementDetailsGallery.Local.FirstOrDefault(u => u.AchievementDetailsGalleryId == model.AchievementDetailsGalleryId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.AchievementDetailsGallery.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
