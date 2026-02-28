using System.ComponentModel.DataAnnotations;

namespace RatingService.Domain.DTOs;

public class GetAccommodationRatingsRequest : PagedRequest
{
    [Required]
    public Guid AccommodationId { get; set; }    
}