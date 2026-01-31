using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Common.Repository
{
    public interface IGenericRepository<T>where T : class
    {
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, bool tracked = true);
        Task<T> GetAsync(Expression<Func<T, bool>> filter, bool tracked = true, string? includeProperties = null);
        Task AddAsync(T entity);
        Task RemoveAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task RemoveRangeAsync(IEnumerable<T> entities);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<string> CallStoreProcedure(string spName, SqlParameter[] parameters);

        Task SaveAsync();
    }
}
