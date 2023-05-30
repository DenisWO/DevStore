using DevStore.Identity.API.Factories;
using DevStore.Identity.API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DevStore.API.Core.Users.Interfaces;
using DevStore.API.Core.Users;
using Microsoft.AspNetCore.Http;

namespace DevStore.Identity.API.Configuration
{
    public static class AppConfigurationExtension
    {
        public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddControllers();

            services.AddScoped<IUserClaimsPrincipalFactory<User>, CustomClaimsFactory>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IAspNetUser, AspNetUser>();

            services.AddAutoMapper(typeof(Startup));

            return services;
        }
        public static IApplicationBuilder UseAppConfiguration(this IApplicationBuilder app, IWebHostEnvironment env) 
        {
            app.UseSwaggerConfiguration();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            return app;
        }
    }
}
