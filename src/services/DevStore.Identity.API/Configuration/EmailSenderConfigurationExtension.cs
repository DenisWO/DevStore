using DS.Email.Services.Interfaces;
using DS.Email.Services;
using DS.Email;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DevStore.Identity.API.Configuration
{
    public static class EmailSenderConfigurationExtension
    {
        public static IServiceCollection AddEmailSenderConfiguration(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddSingleton(configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
            services.AddScoped<IEmailSender, EmailSender>();

            services.Configure<FormOptions>(opt => {
                opt.ValueLengthLimit = int.MaxValue;
                opt.MultipartBodyLengthLimit = int.MaxValue;
                opt.MemoryBufferThreshold = int.MaxValue;
            });

            return services;
        }
    }
}
