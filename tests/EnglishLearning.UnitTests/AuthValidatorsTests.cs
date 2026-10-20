using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Validators;
using FluentAssertions;
using Xunit;

namespace EnglishLearning.UnitTests;

public class AuthValidatorsTests
{
    private readonly RegisterRequestValidator _registerValidator = new();
    private readonly LoginRequestValidator _loginValidator = new();

    [Fact]
    public void RegisterValidator_WithValidRequest_ShouldPass()
    {
        var request = new RegisterRequest(
            "Alice Smith",
            "alice@example.com",
            "StrongPass123",
            "StrongPass123",
            "B1",
            "SpeakingFluency"
        );

        var result = _registerValidator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RegisterValidator_WithMismatchedPasswords_ShouldFail()
    {
        var request = new RegisterRequest(
            "Alice Smith",
            "alice@example.com",
            "StrongPass123",
            "DifferentPass456",
            "B1",
            "SpeakingFluency"
        );

        var result = _registerValidator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }

    [Fact]
    public void RegisterValidator_WithWeakPassword_ShouldFail()
    {
        var request = new RegisterRequest(
            "Alice Smith",
            "alice@example.com",
            "short",
            "short",
            "B1",
            "SpeakingFluency"
        );

        var result = _registerValidator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void LoginValidator_WithValidRequest_ShouldPass()
    {
        var request = new LoginRequest("user@example.com", "Password123");
        var result = _loginValidator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LoginValidator_WithInvalidEmail_ShouldFail()
    {
        var request = new LoginRequest("not-an-email", "Password123");
        var result = _loginValidator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }
}
