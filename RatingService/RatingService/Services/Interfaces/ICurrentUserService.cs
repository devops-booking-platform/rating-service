namespace RatingService.Services.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Role { get; }
    string? Username { get; } 
    string? FirstName { get; } 
    string? LastName { get; } 
    bool IsAuthenticated { get; }
}