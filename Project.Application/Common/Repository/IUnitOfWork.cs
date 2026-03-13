using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Common.Repository
{
    public interface IUnitOfWork
    {
        #region::Authentication

        IRoleRepository roleRepository { get; }
        IContactRepository contactRepository { get; }
        IUsersRepository usersRepository { get; }

        #endregion

        #region::Bananer
        public IBannerRepository bannerRepository { get; }
        #endregion

        #region::ClubDescription
        public IClubDescriptionRepository clubDescriptionRepository { get; }
        public IClubDescriptionImageRepository clubDescriptionImageRepository { get; }
        #endregion
    }
}
