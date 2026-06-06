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
    public class ActivitieDetailsService:GenericService<ActivitieDetails>,IActivitieDetailsRepository
    {
        private readonly ApplicationDbContext _db;
        public ActivitieDetailsService(IConfiguration configuration, ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }
        public async Task<ActivitieDetails> UpdateAsync(ActivitieDetails model)
        {
            var existingId = _db.ActivitieDetails.Local.FirstOrDefault(u => u.ActivitieDetailsId == model.ActivitieDetailsId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.ActivitieDetails.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
