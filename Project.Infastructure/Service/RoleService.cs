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
    public class RoleService:GenericService<Role>,IRoleRepository
    {

        private readonly ApplicationDbContext _context;

        public RoleService(IConfiguration configuration, ApplicationDbContext context):base(configuration, context)
        {
            _context = context;
        }
    }
}
