using System.ComponentModel.DataAnnotations;

namespace RatingService.Domain.DTOs;

public class GetHostRatingsRequest : PagedRequest
{
    [Required]
    public Guid HostId { get; set; }    
}