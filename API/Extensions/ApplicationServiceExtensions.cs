using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration _config)
        {
            //1. if we use AddScoped method if will disposing the http request when it completed.
            //2. if we use AddSingleton method service alive until application alive.
            //3. if we use AddTransient method http request will destory when method is destroy.

            services.AddScoped<ITokenService, TokenService>();

            services.AddDbContext<DataContext>(options =>
            {

                options.UseSqlite(_config.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
    }
}