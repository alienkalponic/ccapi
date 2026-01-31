using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Project.Application.Common.Repository;
using Project.Domain.Utility;
using Project.Infastructure.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infastructure.Service
{
    public class GenericService<T>:IGenericRepository<T>where T : class
    {
        internal DbSet<T> dbSet;
        private readonly ApplicationDbContext _db;
        private List<string> _ExceptionError = new List<string>();
        private SqlConnection _ConnObj;
        private DataTable _dtObj;
        private SqlParameter[] _paramObj;
        private IConfiguration _configuration;
        private readonly DataBaseManagement _dbObj;

        public GenericService(IConfiguration configuration,ApplicationDbContext db)
        {
            _db = db;
            dbSet = _db.Set<T>();
            _dbObj = new DataBaseManagement(configuration, "DB");
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, bool tracked = true)
        {
            IQueryable<T> query = dbSet;
            if (tracked)
            {
                query = query.AsNoTracking();
            }
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (!tracked)
            {
                query = query.AsNoTracking();
            }
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task AddAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            await SaveAsync();

        }

        public async Task RemoveAsync(T entity)
        {
            dbSet.Remove(entity);
            await SaveAsync();
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await dbSet.AddRangeAsync(entities);
            await SaveAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
            await SaveAsync();



        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await dbSet.CountAsync(predicate);
        }

        public async Task<List<T>> ListProcedure<T>(string spName, Hashtable htParam) where T : class, new()
        {
            var results = new List<T>();

            using (var command = _db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = spName;
                command.CommandType = CommandType.StoredProcedure;
                IDictionaryEnumerator iEnum = htParam.GetEnumerator();

                while (iEnum.MoveNext())
                {
                    var sqlParam = new SqlParameter(iEnum.Key.ToString(), iEnum.Value ?? DBNull.Value);
                    command.Parameters.Add(sqlParam);
                }

                if (command.Connection.State != ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }

                try
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var properties = typeof(T).GetProperties();

                        while (await reader.ReadAsync())
                        {
                            var entity = new T();

                            foreach (var prop in properties)
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal(prop.Name)))
                                {
                                    var value = reader[prop.Name];
                                    prop.SetValue(entity, value);
                                }
                            }

                            results.Add(entity);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception (using your preferred logging framework)
                    //throw new DataAccessException("An error occurred while executing the stored procedure.", ex);
                }
                finally
                {
                    await command.Connection.CloseAsync();
                }
            }

            return results;
        }

        public async Task<string> CallStoreProcedure(string spName, SqlParameter[] parameters)
        {
            //_dtObj = new DataTable();
            string _dtObj = _dbObj.Select(spName, parameters);
            return _dtObj;
        }
        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
