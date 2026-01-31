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
    public class UsersService : GenericService<Users>, IUsersRepository
    {
        private readonly ApplicationDbContext _db;

        public UsersService(IConfiguration configuration, ApplicationDbContext db):base(configuration, db) 
        {
            _db = db;
        }

        public async Task<Users> UpdateAsync(Users model)
        {
            var existingId = _db.Users.Local.FirstOrDefault(u => u.UserId == model.UserId);
            if (existingId != null)
            {
                _db.Entry(existingId).State = EntityState.Detached;
            }
            _db.Users.Update(model);
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
