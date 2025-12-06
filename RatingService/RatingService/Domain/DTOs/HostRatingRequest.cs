namespace RatingService.Domain.DTOs;

public class HostRatingRequest
{
    public Guid? Id { get; set; }
    public Guid HostId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}