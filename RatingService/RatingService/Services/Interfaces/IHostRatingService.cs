using RatingService.Domain.DTOs;

namespace RatingService.Services.Interfaces;

public interface IHostRatingService
{
    Task CreateOrUpdateHostRating(HostRatingRequest request);
    Task DeleteHostRating(Guid id);
    Task<PagedResult<RatingResponse>> GetRatings(GetHostRatingsRequest request);
    Task<GetRatingResponse> GetRating(Guid id);
}