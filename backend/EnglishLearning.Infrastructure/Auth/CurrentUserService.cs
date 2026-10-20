using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EnglishLearning.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace EnglishLearning.Infrastructure.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return null;

            var subClaim = user.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                           ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(subClaim, out var userId) ? userId : null;
        }
    }

    public string? Email
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue(JwtRegisteredClaimNames.Email) 
                   ?? user?.FindFirstValue(ClaimTypes.Email);
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
