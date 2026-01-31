using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Application;
using Project.Application.Common.Repository;
using Project.Domain.Utility;
using Project.Infastructure.Data;
using Project.Infastructure.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infastructure
{
    public static class ConfigurationService
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuraton)
        {
            services.AddApplicationService();
            services.AddDbContext<ApplicationDbContext>(options =>

                options.UseSqlServer(configuraton.GetConnectionString("DB") ?? throw new InvalidOperationException("Connection string 'DB not found'"))
            );
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<LogService>();
            services.AddSingleton<IApiResponseService, ApiResponseService>();
            return services;
        }
    }
}
