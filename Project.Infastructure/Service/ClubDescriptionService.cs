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
    public class ClubDescriptionService:GenericService<ClubDescription>,IClubDescriptionRepository
    {
        private readonly ApplicationDbContext _db;
        public ClubDescriptionService(IConfiguration configuration, ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }
        public async Task<ClubDescription> UpdateAsync(ClubDescription model)
        {
            var existingId = _db.ClubDescription.Local.FirstOrDefault(u => u.ClubDescriptionId == model.ClubDescriptionId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.ClubDescription.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
