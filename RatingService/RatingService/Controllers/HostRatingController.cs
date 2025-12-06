using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RatingService.Domain.DTOs;
using RatingService.Services.Interfaces;

namespace RatingService.Controllers;

[Route("api/host-ratings")]
[ApiController]
public class HostRatingController(IHostRatingService hostRatingService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> CreateOrUpdateHostRating([FromBody] HostRatingRequest request)
    {
        await hostRatingService.CreateOrUpdateHostRating(request);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpDelete]
    [Route("{id:guid}")]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> DeleteHostRating(Guid id)
    {
        await hostRatingService.DeleteHostRating(id);
        return StatusCode(StatusCodes.Status201Created);
    }
}