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
    public class ActivityRegistrationService:GenericService<ActivityRegistration>,IActivityRegistrationRepository
    {
        private readonly ApplicationDbContext _db;
        public ActivityRegistrationService(IConfiguration configuration, ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }

        public async Task<ActivityRegistration> UpdateAsync(ActivityRegistration model)
        {
            var existingId = _db.ActivityRegistration.Local.FirstOrDefault(u => u.RegistrationId == model.RegistrationId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.ActivityRegistration.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
