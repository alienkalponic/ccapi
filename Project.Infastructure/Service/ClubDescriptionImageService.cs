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
    public class ClubDescriptionImageService:GenericService<ClubDescriptionImage>, IClubDescriptionImageRepository
    {
        private readonly ApplicationDbContext _db;
        public ClubDescriptionImageService(IConfiguration configuration, ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }
        public async Task<ClubDescriptionImage> UpdateAsync(ClubDescriptionImage model)
        {
            var existingId = _db.ClubDescriptionImage.Local.FirstOrDefault(u => u.ClubDescriptionImageId == model.ClubDescriptionImageId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.ClubDescriptionImage.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
