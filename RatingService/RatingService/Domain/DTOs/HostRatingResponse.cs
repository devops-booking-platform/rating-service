namespace RatingService.Domain.DTOs;

public class RatingResponse
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string GuestFullName { get; set; } = string.Empty;
    public Guid GuestId { get; set; }
    public Guid? HostId { get; set; }
    public Guid? AccommodationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastChangedAt { get; set; }
}