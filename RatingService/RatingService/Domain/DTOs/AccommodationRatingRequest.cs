using System.ComponentModel.DataAnnotations;

namespace RatingService.Domain.DTOs;

public class AccommodationRatingRequest
{
    public Guid? Id { get; set; }
    public Guid AccommodationId { get; set; }
    public int Rating { get; set; }

    [MaxLength(AccommodationRating.CommentMaxLength)]
    public string Comment { get; set; } = string.Empty;
}