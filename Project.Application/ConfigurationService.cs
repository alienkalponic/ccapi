using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Project.Domain.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application
{
    public static class ConfigurationService
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MapConfig));
            return services; 
        }
    }
}
