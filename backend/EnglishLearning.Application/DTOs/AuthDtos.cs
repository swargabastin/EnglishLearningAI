namespace EnglishLearning.Application.DTOs;

public record RegisterRequest(
    string Name,
    string Email,
    string Password,
    string ConfirmPassword,
    string EnglishLevel,
    string LearningGoal
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthenticatedUserDto(
    Guid Id,
    string Name,
    string Email,
    string EnglishLevel,
    string LearningGoal,
    DateTime CreatedAt
);

public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    AuthenticatedUserDto User
);

public record UserProfileDto(
    Guid Id,
    string Name,
    string Email,
    string EnglishLevel,
    string LearningGoal,
    DateTime CreatedAt
);
