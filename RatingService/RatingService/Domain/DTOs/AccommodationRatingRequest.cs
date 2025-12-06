using System.ComponentModel.DataAnnotations;

namespace RatingService.Domain.DTOs;

public class AccommodationRatingRequest
{
    public Guid? Id { get; set; }
    public Guid AccommodationId { get; set; }
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [MaxLength(AccommodationRating.CommentMaxLength)]
    public string Comment { get; set; } = string.Empty;
}