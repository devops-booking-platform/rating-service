namespace RatingService.Domain.DTOs;

public class RatingResponse
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string GuestFullName { get; set; } = string.Empty;
}