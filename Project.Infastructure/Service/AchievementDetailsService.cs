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
    public class AchievementDetailsService:GenericService<AchievementDetails>,IAchievementDetailsRepository
    {
        private readonly ApplicationDbContext _db;
        public AchievementDetailsService(IConfiguration configuration, ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }
        public async Task<AchievementDetails> UpdateAsync(AchievementDetails model)
        {
            var existingId = _db.AchievementDetails.Local.FirstOrDefault(u => u.AchievementDetailsId == model.AchievementDetailsId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.AchievementDetails.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
