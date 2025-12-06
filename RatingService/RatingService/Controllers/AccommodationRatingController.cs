using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RatingService.Domain.DTOs;
using RatingService.Services.Interfaces;

namespace RatingService.Controllers;

[Route("api/accommodation-ratings")]
[ApiController]
public class AccommodationRatingController(IAccommodationRatingService accommodationRatingService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> CreateOrUpdate([FromBody] AccommodationRatingRequest request)
    {
        await accommodationRatingService.CreateOrUpdateAccommodationRating(request);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await accommodationRatingService.DeleteAccommodationRating(id);
        return NoContent();
    }
}