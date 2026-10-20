using EnglishLearning.Domain.Entities;

namespace EnglishLearning.Application.Interfaces.Infrastructure;

public interface IPasswordHasherService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string hashedPassword, string providedPassword);
}
