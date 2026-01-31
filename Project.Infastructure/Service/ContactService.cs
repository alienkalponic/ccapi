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
    public class ContactService : GenericService<Contact>, IContactRepository
    {
        private readonly ApplicationDbContext _db;

        public ContactService(IConfiguration configuration, ApplicationDbContext db):base(configuration, db)
        {
            _db = db;
        }

        public async Task<Contact> UpdateAsync(Contact model)
        {
            var existingId = _db.Contact.Local.FirstOrDefault(u => u.ContactId == model.ContactId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.Contact.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
