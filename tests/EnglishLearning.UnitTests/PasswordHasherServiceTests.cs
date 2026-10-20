using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Auth;
using FluentAssertions;
using Xunit;

namespace EnglishLearning.UnitTests;

public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _hasher = new();

    [Fact]
    public void HashPassword_ShouldGenerate_NonEmptyDifferentHashFromPlaintext()
    {
        var user = new User { Email = "test@example.com" };
        var password = "SecurePassword123";

        var hash = _hasher.HashPassword(user, password);

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var user = new User { Email = "test@example.com" };
        var password = "SecurePassword123";

        var hash = _hasher.HashPassword(user, password);
        var isValid = _hasher.VerifyPassword(user, hash, password);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        var user = new User { Email = "test@example.com" };
        var password = "SecurePassword123";

        var hash = _hasher.HashPassword(user, password);
        var isValid = _hasher.VerifyPassword(user, hash, "WrongPassword456");

        isValid.Should().BeFalse();
    }
}
