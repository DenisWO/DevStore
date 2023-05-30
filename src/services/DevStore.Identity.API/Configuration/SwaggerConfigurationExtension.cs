﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;

namespace DevStore.Identity.API.Configuration
{
    public static class SwaggerConfigurationExtension
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Swagger UI",
                    Description = "Identity Documentation",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Denis Oliveira",
                        Email = "email@[domain].com",
                        Url = new Uri("https://twitter.com/[twitterName]"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "License Information",
                        Url = new Uri("https://example.com/license"),
                    }
                });
            });

            return services;
        }
        public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Showing API V1");
            });

            return app;
        }
    }
}
