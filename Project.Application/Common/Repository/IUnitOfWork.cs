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
    }
}
