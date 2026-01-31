using Microsoft.EntityFrameworkCore;
using Project.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infastructure.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options)
        {
            
        }
        #region::Authentication
        public DbSet<Role> Role { get; set; }
        public DbSet<Users> Users {  get; set; }
        public DbSet<Contact> Contact {  get; set; }
        #endregion
    }
}
