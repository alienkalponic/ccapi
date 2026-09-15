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
    public class AboutPersonService:GenericService<AboutPerson>, IAboutPersonRepository
    {
        private readonly Data.ApplicationDbContext _db;
        public AboutPersonService(IConfiguration configuration, Data.ApplicationDbContext db) : base(configuration, db)
        {
            _db = db;
        }
        public async Task<AboutPerson> UpdateAsync(AboutPerson model)
        {
            var existingId = _db.AboutPerson.Local.FirstOrDefault(u => u.PersonId == model.PersonId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            _db.AboutPerson.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
