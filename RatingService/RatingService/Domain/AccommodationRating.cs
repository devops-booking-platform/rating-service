using RatingService.Domain.DTOs;

namespace RatingService.Domain;

public class AccommodationRating : EntityWithGuidId
{
    public const int CommentMaxLength = 255;
    public const int ViewingPropertiesMaxLength = 100;
    public Guid AccommodationId { get; set; }
    public Guid GuestId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastChangedAt { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    // for viewing purpose
    public string GuestFirstName { get; set; } = string.Empty;
    public string GuestLastName { get; set; } = string.Empty;
    public string GuestUsername { get; set; } = string.Empty;

    public static AccommodationRating Create(AccommodationRatingRequest request, Guid guestId, string? guestFirstName,
        string? guestLastName, string? guestUsername)
        => new()
        {
            AccommodationId = request.AccommodationId,
            Rating = request.Rating,
            GuestId = guestId,
            GuestFirstName = guestFirstName ?? string.Empty,
            GuestLastName = guestLastName ?? string.Empty,
            GuestUsername = guestUsername ?? string.Empty,
            Comment = request.Comment,
            CreatedAt = DateTimeOffset.UtcNow,
            LastChangedAt = DateTimeOffset.UtcNow
        };

    public void Update(AccommodationRatingRequest request, string? guestFirstName,
        string? guestLastName, string? guestUsername)
    {
        Rating = request.Rating;
        Comment = request.Comment;
        LastChangedAt = DateTimeOffset.UtcNow;
        GuestFirstName = guestFirstName ?? string.Empty;
        GuestLastName = guestLastName ?? string.Empty;
        GuestUsername = guestUsername ?? string.Empty;
    }
}