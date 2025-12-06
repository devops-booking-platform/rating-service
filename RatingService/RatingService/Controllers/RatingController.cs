using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RatingService.Domain.DTOs;
using RatingService.Services.Interfaces;

namespace RatingService.Controllers;

[Route("api/ratings")]
[ApiController]
public class RatingController(IRatingService ratingService) : ControllerBase
{
    [HttpPost]
    [Route("host")]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> CreateOrUpdateHostRating([FromBody] HostRatingRequest request)
    {
        await ratingService.CreateOrUpdateHostRating(request);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost]
    [Route("accommodation")]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> CreateOrUpdateAccommodationRating([FromBody] AccommodationRatingRequest request)
    {
        await ratingService.CreateOrUpdateAccommodationRating(request);
        return StatusCode(StatusCodes.Status201Created);
    }
}