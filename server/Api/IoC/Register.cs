using Api.Auth.Validators;
using Api.Shared.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Auth.IoC;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api.IoC;

public static class Register
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<RegisterUserRequestValidator>();

        // JWT auth
        var jwtOptions = configuration.GetSection("JwtOptions");
        var key = Encoding.UTF8.GetBytes(jwtOptions["Key"]!);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions["Issuer"],
                ValidAudience = jwtOptions["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        services.AddAuthentication();

        // Rate limiting
        services.AddRateLimiter(options => options.ConfigureRateLimiting(configuration));

        services.AddAuthInfrastructureServices(configuration);

        return services;
    }
}
