using EnglishLearning.Application.DTOs;

namespace EnglishLearning.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
