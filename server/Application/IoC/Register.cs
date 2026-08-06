using Application.Auth.Commands;
using Application.Auth.DTOs;
using Application.Auth.Handlers;
using Application.LanguagePractice.Commands;
using Application.LanguagePractice.DTOs;
using Application.LanguagePractice.Handlers;
using Application.LanguagePractice.Interfaces;
using Application.LanguagePractice.Queries;
using Application.LanguagePractice.Services;
using Application.Shared.Interfaces;
using Application.Submissions.Commands;
using Application.Submissions.DTOs;
using Application.Submissions.Handlers;
using Application.Submissions.Queries;
using Domain.LanguagePractice.Events;
using Domain.LanguagePractice.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IoC;

public static class Register
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateSubmissionCommand, Guid>, CreateSubmissionCommandHandler>();
        services.AddScoped<ICommandHandler<LoginUserCommand, AuthResponse>, LoginUserCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterUserCommand, AuthResponse>, RegisterUserCommandHandler>();
        services.AddScoped<IQueryHandler<GetSubmissionsQuery, IEnumerable<SubmissionResponse>>, GetSubmissionsQueryHandler>();
        services.AddScoped<ICommandHandler<SetPracticeLanguageCommand>, SetPracticeLanguageCommandHandler>();
        services.AddScoped<IQueryHandler<GetUserLanguageQuery, string>, GetUserLanguageQueryHandler>();
        services.AddScoped<IQueryHandler<GetLemmaStatsQuery, IEnumerable<LemmaStatistic>>, GetUserLemmaStatsQueryHandler>();
        services.AddScoped<IQueryHandler<GetActiveLanguageStatsQuery, LanguageStatsResponse>, GetActiveLanguageStatsQueryHandler>();
        services.AddScoped<ILanguageValidationService, LanguageValidationService>();
        return services;
    }

    public static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        services.AddScoped<IEventHandler<LanguageSubmissionCreatedEvent>, SubmissionCreatedEventHandler>();
        services.AddScoped<IEventHandler<LanguageSubmissionAnalysedEvent>, SubmissionAnalysedEventHandler>();

        return services;
    }
}
