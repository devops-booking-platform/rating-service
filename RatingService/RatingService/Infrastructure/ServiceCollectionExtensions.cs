using Microsoft.AspNetCore.Authentication.JwtBearer;
using RatingService.Repositories;
using RatingService.Repositories.Interfaces;
using RatingService.Services;
using RatingService.Services.Interfaces;

namespace RatingService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccommodationServiceDependencies(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }

    public static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT"
                });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }
}