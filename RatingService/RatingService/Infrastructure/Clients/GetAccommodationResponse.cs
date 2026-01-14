namespace RatingService.Infrastructure.Clients;

public class GetAccommodationResponse
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
