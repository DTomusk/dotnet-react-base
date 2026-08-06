using Application.Auth.Commands;
using Application.Auth.DTOs;
using Application.Auth.Handlers;
using Application.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IoC;

public static class Register
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<LoginUserCommand, AuthResponse>, LoginUserCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterUserCommand, AuthResponse>, RegisterUserCommandHandler>();
        return services;
    }

    public static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        return services;
    }
}
