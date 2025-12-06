using System.ComponentModel.DataAnnotations;

namespace RatingService.Domain.DTOs;

public class HostRatingRequest
{
    public Guid? Id { get; set; }
    public Guid HostId { get; set; }
    public int Rating { get; set; }
    [MaxLength(HostRating.CommentMaxLength)]
    public string Comment { get; set; } = string.Empty;
}