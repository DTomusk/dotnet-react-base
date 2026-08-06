using Application.Auth.Commands;
using Application.Auth.DTOs;
using Application.Auth.Interfaces;
using Application.LanguagePractice.Interfaces;
using Application.Shared.Interfaces;
using Domain.Auth.Entities;
using Domain.Auth.Events;
using Domain.LanguagePractice.Entities;
using Domain.Shared.Results;

namespace Application.Auth.Handlers;

public class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    ILanguageLearnerRepository languageLearnerRepository,
    IDomainEventPublisher eventPublisher,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly ILanguageLearnerRepository _languageLearnerRepository = languageLearnerRepository;
    private readonly IDomainEventPublisher _eventPublisher = eventPublisher;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<AuthResponse>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByDisplayNameAsync(command.DisplayName, cancellationToken);
        if (existingUser != null)
            return Result<AuthResponse>.Failure(new Error("User with the same display name already exists.", ErrorType.Validation));

        var passwordHash = _passwordHasher.HashPassword(command.Password);
        var user = User.Create(command.DisplayName, passwordHash);

        await _userRepository.CreateAsync(user, cancellationToken);

        var languageLearnerCreationResult = await CreateLanguageLearner(user, cancellationToken);
        if (languageLearnerCreationResult.IsFailure)
            return Result<AuthResponse>.Failure(languageLearnerCreationResult.Error);

        var token = _tokenGenerator.GenerateToken(user.Id, user.DisplayName);

        await _eventPublisher.PublishAsync(new UserCreatedEvent()
        {
            UserId = user.Id,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt
        }, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(user.Id, user.DisplayName, token));
    }

    // This crosses bounded context, consider how to handle
    private async Task<Result> CreateLanguageLearner(User user, CancellationToken cancellationToken)
    {
        var languageLearnerResult = LanguageLearner.Create(user.Id);
        if (languageLearnerResult.IsFailure)
            return Result.Failure(languageLearnerResult.Error);

        await _languageLearnerRepository.CreateAsync(languageLearnerResult.Value, cancellationToken);
        return Result.Success();
    }
}
