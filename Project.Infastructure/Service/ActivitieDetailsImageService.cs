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
    public class ActivitieDetailsImageService:GenericService<ActivitieDetailsImage>, IActivitieDetailsImageRepository
    {
        private readonly ApplicationDbContext _db;
        public ActivitieDetailsImageService(IConfiguration configuration, ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }
        public async Task<ActivitieDetailsImage> UpdateAsync(ActivitieDetailsImage model)
        {
            var existingId = _db.ActivitieDetailsImage.Local.FirstOrDefault(u => u.ActivitieDetailsImageId == model.ActivitieDetailsImageId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.ActivitieDetailsImage.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
