using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnglishLearning.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasherService> _hasherMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGenMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _hasherMock = new Mock<IPasswordHasherService>();
        _tokenGenMock = new Mock<IJwtTokenGenerator>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _authService = new AuthService(
            _userRepoMock.Object,
            _unitOfWorkMock.Object,
            _hasherMock.Object,
            _tokenGenMock.Object,
            _currentUserMock.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var request = new RegisterRequest(
            "Bob Marley",
            "bob@example.com",
            "Pass1234!",
            "Pass1234!",
            "B1",
            "SpeakingFluency"
        );

        _userRepoMock.Setup(r => r.GetByEmailAsync("bob@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _hasherMock.Setup(h => h.HashPassword(It.IsAny<User>(), "Pass1234!"))
            .Returns("hashed_secret");

        _tokenGenMock.Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns(("jwt_mock_token", DateTime.UtcNow.AddHours(24)));

        // Act
        var response = await _authService.RegisterAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().Be("jwt_mock_token");
        response.User.Email.Should().Be("bob@example.com");
        response.User.Name.Should().Be("Bob Marley");
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrow_DuplicateEmailException()
    {
        // Arrange
        var request = new RegisterRequest(
            "Bob Marley",
            "existing@example.com",
            "Pass1234!",
            "Pass1234!",
            "B1",
            "SpeakingFluency"
        );

        _userRepoMock.Setup(r => r.GetByEmailAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "existing@example.com" });

        // Act
        var act = () => _authService.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<DuplicateEmailException>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var request = new LoginRequest("user@example.com", "CorrectPass123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "John",
            Email = "user@example.com",
            PasswordHash = "hashed_pass"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _hasherMock.Setup(h => h.VerifyPassword(user, "hashed_pass", "CorrectPass123"))
            .Returns(true);

        _tokenGenMock.Setup(t => t.GenerateToken(user))
            .Returns(("login_token", DateTime.UtcNow.AddHours(24)));

        // Act
        var response = await _authService.LoginAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().Be("login_token");
        response.User.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrow_InvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest("user@example.com", "WrongPassword");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hashed_pass"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _hasherMock.Setup(h => h.VerifyPassword(user, "hashed_pass", "WrongPassword"))
            .Returns(false);

        // Act
        var act = () => _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ShouldThrow_InvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest("unknown@example.com", "Password123");

        _userRepoMock.Setup(r => r.GetByEmailAsync("unknown@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }
}
