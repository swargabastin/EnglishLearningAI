using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EnglishLearning.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[EnableRateLimiting("AuthRateLimit")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var response = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response, "Registration successful."));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response, "Login successful."));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var profile = await _authService.GetCurrentUserProfileAsync(cancellationToken);
        return Ok(ApiResponse<UserProfileDto>.Ok(profile));
    }

    [Authorize]
    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<AvatarUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadAvatar(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Avatar",
                Detail = "Please provide an image file in the 'file' form field.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "File Too Large",
                Detail = "Avatar image cannot exceed 5MB.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unsupported Image Format",
                Detail = "Supported formats are JPG, PNG, WEBP, and GIF.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var userId = _currentUserService.UserId ?? Guid.Empty;
        var avatarsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "avatars");
        if (!Directory.Exists(avatarsDir))
        {
            Directory.CreateDirectory(avatarsDir);
        }

        foreach (var existing in Directory.GetFiles(avatarsDir, $"{userId}.*"))
        {
            try { System.IO.File.Delete(existing); } catch { }
        }

        var filePath = Path.Combine(avatarsDir, $"{userId}{extension}");
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var avatarUrl = $"/api/v1/auth/avatar/{userId}?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        return Ok(ApiResponse<AvatarUploadResponse>.Ok(new AvatarUploadResponse(avatarUrl, "Avatar uploaded successfully.")));
    }

    [HttpGet("avatar/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAvatar([FromRoute] Guid userId)
    {
        var avatarsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "avatars");
        if (!Directory.Exists(avatarsDir))
        {
            return NotFound();
        }

        var existingFiles = Directory.GetFiles(avatarsDir, $"{userId}.*");
        if (existingFiles.Length == 0)
        {
            return NotFound();
        }

        var filePath = existingFiles[0];
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var mimeType = extension switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };

        return PhysicalFile(filePath, mimeType);
    }
}
