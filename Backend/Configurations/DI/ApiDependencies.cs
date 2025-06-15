using System.Text;
using Backend.Api.Hubs;
using Backend.Domain.Abstractions;
using Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Configurations.DI;

public static class ApiDependencies
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<JwtService>();
        
        services.AddSignalR()
            .AddJsonProtocol()
            .AddHubOptions<QuizHub>(options => {
                options.EnableDetailedErrors = true;
            });
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? string.Empty))
                };
            });
        
        services.AddCors(options => 
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true)
                    .AllowCredentials();
            });
        });
        
        return services;
    }
}