using System.IdentityModel.Tokens.Jwt;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Auth;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EnglishLearning.UnitTests;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "UnitTestingSecretKey_MustBeAtLeast32BytesLong_2026!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpiryMinutes", "60" }
            })
            .Build();

        _generator = new JwtTokenGenerator(config);
    }

    [Fact]
    public void GenerateToken_ShouldCreate_ValidJwtWithExpectedClaims()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john.doe@example.com",
            EnglishLevel = EnglishProficiencyLevel.B2,
            LearningGoal = LearningGoal.WorkplaceEnglish
        };

        // Act
        var (token, expiresAt) = _generator.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        expiresAt.Should().BeAfter(DateTime.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("TestIssuer");
        jwt.Audiences.Should().Contain("TestAudience");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == "level" && c.Value == "B2");
        jwt.Claims.Should().Contain(c => c.Type == "goal" && c.Value == "WorkplaceEnglish");
    }
}
