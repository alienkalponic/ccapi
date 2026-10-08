using Project.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Common.Repository
{
    public interface ICourseAccountantRepository:IGenericRepository<CourseAccountant>
    {
        Task<CourseAccountant> UpdateAsync(CourseAccountant model);
    }
}
