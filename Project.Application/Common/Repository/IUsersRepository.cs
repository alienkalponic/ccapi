using Project.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Common.Repository
{
    public interface IUsersRepository:IGenericRepository<Users>
    {
        Task<Users> UpdateAsync(Users model);
    }
}
