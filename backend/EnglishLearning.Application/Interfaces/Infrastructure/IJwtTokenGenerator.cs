using EnglishLearning.Domain.Entities;

namespace EnglishLearning.Application.Interfaces.Infrastructure;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
