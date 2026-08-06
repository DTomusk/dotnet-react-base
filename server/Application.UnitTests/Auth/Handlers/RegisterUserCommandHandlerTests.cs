using Application.Auth.Commands;
using Application.Auth.Handlers;
using Application.Auth.Interfaces;
using Application.LanguagePractice.Interfaces;
using Application.Shared.Interfaces;
using Domain.Auth.Entities;
using Domain.Auth.Events;
using Domain.Shared.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Auth.Handlers;

public class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ILanguageLearnerRepository _languageLearnerRepository;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;

    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenGenerator = Substitute.For<ITokenGenerator>();
        _languageLearnerRepository = Substitute.For<ILanguageLearnerRepository>();
        _eventPublisher = Substitute.For<IDomainEventPublisher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher, _tokenGenerator, _languageLearnerRepository, _eventPublisher, _unitOfWork);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Success_With_AuthResponse_When_User_Registered_Successfully()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value.DisplayName.Should().Be(displayName);
        result.Value.Token.Should().Be(token);
        result.Value.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Failure_When_User_Already_Exists()
    {
        // Arrange
        var displayName = "existinguser";
        var password = "password123";
        var passwordHash = "hashed_password";

        var existingUser = User.Create(displayName, passwordHash);
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be("User with the same display name already exists.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task HandleAsync_Should_Call_GetByDisplayNameAsync_To_Check_Existing_User()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _userRepository.Received(1).GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Hash_Password_With_Correct_Value()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _passwordHasher.Received(1).HashPassword(password);
    }

    [Fact]
    public async Task HandleAsync_Should_Create_User_In_Repository()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _userRepository.Received(1).CreateAsync(
            Arg.Is<User>(u => u.DisplayName == displayName && u.PasswordHash == passwordHash),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Generate_Token_With_User_Id_And_DisplayName()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _tokenGenerator.Received(1).GenerateToken(Arg.Any<Guid>(), displayName);
    }

    [Fact]
    public async Task HandleAsync_Should_Publish_UserCreatedEvent_With_Correct_Data()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<UserCreatedEvent>(e =>
                e.DisplayName == displayName &&
                e.UserId != Guid.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Commit_Unit_Of_Work_After_Publishing_Event()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Not_Create_User_When_User_Already_Exists()
    {
        // Arrange
        var displayName = "existinguser";
        var password = "password123";
        var passwordHash = "hashed_password";

        var existingUser = User.Create(displayName, passwordHash);
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Not_Hash_Password_When_User_Already_Exists()
    {
        // Arrange
        var displayName = "existinguser";
        var password = "password123";
        var passwordHash = "hashed_password";

        var existingUser = User.Create(displayName, passwordHash);
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _passwordHasher.DidNotReceive().HashPassword(Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_Should_Not_Generate_Token_When_User_Already_Exists()
    {
        // Arrange
        var displayName = "existinguser";
        var password = "password123";
        var passwordHash = "hashed_password";

        var existingUser = User.Create(displayName, passwordHash);
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _tokenGenerator.DidNotReceive().GenerateToken(Arg.Any<Guid>(), Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_Should_Not_Publish_Event_When_User_Already_Exists()
    {
        // Arrange
        var displayName = "existinguser";
        var password = "password123";
        var passwordHash = "hashed_password";

        var existingUser = User.Create(displayName, passwordHash);
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _eventPublisher.DidNotReceive().PublishAsync(Arg.Any<UserCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Not_Commit_Unit_Of_Work_When_User_Already_Exists()
    {
        // Arrange
        var displayName = "existinguser";
        var password = "password123";
        var passwordHash = "hashed_password";

        var existingUser = User.Create(displayName, passwordHash);
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Pass_CancellationToken_To_Repository_GetByDisplayNameAsync()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var cancellationToken = new CancellationToken();
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, cancellationToken)
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(cancellationToken)
            .Returns(1);

        // Act
        await _handler.HandleAsync(command, cancellationToken);

        // Assert
        await _userRepository.Received(1).GetByDisplayNameAsync(displayName, cancellationToken);
    }

    [Fact]
    public async Task HandleAsync_Should_Pass_CancellationToken_To_Repository_CreateAsync()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var cancellationToken = new CancellationToken();
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, cancellationToken)
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(cancellationToken)
            .Returns(1);

        // Act
        await _handler.HandleAsync(command, cancellationToken);

        // Assert
        await _userRepository.Received(1).CreateAsync(
            Arg.Any<User>(),
            cancellationToken);
    }

    [Fact]
    public async Task HandleAsync_Should_Pass_CancellationToken_To_EventPublisher()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var cancellationToken = new CancellationToken();
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, cancellationToken)
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(cancellationToken)
            .Returns(1);

        // Act
        await _handler.HandleAsync(command, cancellationToken);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<UserCreatedEvent>(),
            cancellationToken);
    }

    [Fact]
    public async Task HandleAsync_Should_Pass_CancellationToken_To_UnitOfWork()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var cancellationToken = new CancellationToken();
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, cancellationToken)
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(cancellationToken)
            .Returns(1);

        // Act
        await _handler.HandleAsync(command, cancellationToken);

        // Assert
        await _unitOfWork.Received(1).CommitAsync(cancellationToken);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_AuthResponse_With_Correct_UserId()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Value.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_AuthResponse_With_Correct_DisplayName()
    {
        // Arrange
        var displayName = "newuser123";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Value.DisplayName.Should().Be(displayName);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_AuthResponse_With_Generated_Token()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "generated_jwt_token_12345";
        var command = new RegisterUserCommand(displayName, password);

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Value.Token.Should().Be(token);
    }

    [Fact]
    public async Task HandleAsync_Should_Publish_Event_With_CreatedAt_From_User()
    {
        // Arrange
        var displayName = "newuser";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var command = new RegisterUserCommand(displayName, password);
        var beforeCreation = DateTime.UtcNow;

        _userRepository.GetByDisplayNameAsync(displayName, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasher.HashPassword(password)
            .Returns(passwordHash);

        _tokenGenerator.GenerateToken(Arg.Any<Guid>(), displayName)
            .Returns(token);

        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _handler.HandleAsync(command);
        var afterCreation = DateTime.UtcNow;

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<UserCreatedEvent>(e =>
                e.CreatedAt >= beforeCreation &&
                e.CreatedAt <= afterCreation),
            Arg.Any<CancellationToken>());
    }
}
