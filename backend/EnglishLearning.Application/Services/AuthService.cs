using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Exceptions;

namespace EnglishLearning.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasherService passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser != null)
        {
            throw new DuplicateEmailException(normalizedEmail);
        }

        if (!Enum.TryParse<EnglishProficiencyLevel>(request.EnglishLevel, true, out var level))
        {
            level = EnglishProficiencyLevel.B1;
        }

        if (!Enum.TryParse<LearningGoal>(request.LearningGoal, true, out var goal))
        {
            goal = LearningGoal.SpeakingFluency;
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            EnglishLevel = level,
            LearningGoal = goal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse(
            token,
            expiresAt,
            new AuthenticatedUserDto(
                user.Id,
                user.Name,
                user.Email,
                user.EnglishLevel.ToString(),
                user.LearningGoal.ToString(),
                user.CreatedAt
            )
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user == null)
        {
            throw new InvalidCredentialsException();
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(user, user.PasswordHash, request.Password);
        if (!isPasswordValid)
        {
            throw new InvalidCredentialsException();
        }

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse(
            token,
            expiresAt,
            new AuthenticatedUserDto(
                user.Id,
                user.Name,
                user.Email,
                user.EnglishLevel.ToString(),
                user.LearningGoal.ToString(),
                user.CreatedAt
            )
        );
    }

    public async Task<UserProfileDto> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new InvalidCredentialsException("User is not authenticated.");
        }

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException(nameof(User), _currentUserService.UserId.Value);
        }

        var avatarsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "avatars");
        string? avatarUrl = null;
        if (Directory.Exists(avatarsDir) && Directory.GetFiles(avatarsDir, $"{user.Id}.*").Length > 0)
        {
            avatarUrl = $"/api/v1/auth/avatar/{user.Id}";
        }

        return new UserProfileDto(
            user.Id,
            user.Name,
            user.Email,
            user.EnglishLevel.ToString(),
            user.LearningGoal.ToString(),
            user.CreatedAt,
            avatarUrl
        );
    }
}
