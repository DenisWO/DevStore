using DS.Identity.API.Data;
using DS.Identity.API.Data.Repositories;
using DS.Identity.API.Data.Repositories.Interfaces;
using DS.Identity.API.Services;

namespace DS.Identity.API.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<UsersContext>();            

            return services;
        }
    }
}
