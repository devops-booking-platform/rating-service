using System.Security.Claims;
using RatingService.Services.Interfaces;

namespace RatingService.Services;

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public Guid? UserId => Guid.Parse(accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    public string? Role => accessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public bool IsAuthenticated =>
        accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    
    public string? Username => accessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    
    public string? FirstName =>
        accessor.HttpContext?.User?.FindFirst(ClaimTypes.GivenName)?.Value;

    public string? LastName =>
        accessor.HttpContext?.User?.FindFirst(ClaimTypes.Surname)?.Value;
}