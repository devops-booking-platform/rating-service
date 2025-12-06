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
}