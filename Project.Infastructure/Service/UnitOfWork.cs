using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Project.Application.Common.Repository;
using Project.Infastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infastructure.Service
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        #region::Authentication

        public IRoleRepository roleRepository { get; private set; }

        public IContactRepository contactRepository { get; private set; }

        public IUsersRepository usersRepository { get; private set; }



        #endregion

        #region::Banner
        public IBannerRepository bannerRepository { get; private set; }
        #endregion
        public UnitOfWork(IConfiguration configuration, ApplicationDbContext db)
        {
            _db = db;
            _configuration = configuration;
            roleRepository = new RoleService(configuration, _db);
            contactRepository = new ContactService(configuration,_db);
            usersRepository=new UsersService(configuration,_db);
            bannerRepository=new BannerService(configuration, _db);
        }

        
    }
}
