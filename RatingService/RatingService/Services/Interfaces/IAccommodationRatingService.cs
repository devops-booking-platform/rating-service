using RatingService.Domain.DTOs;

namespace RatingService.Services.Interfaces;

public interface IAccommodationRatingService
{
    Task CreateOrUpdateAccommodationRating(AccommodationRatingRequest request);
    Task DeleteAccommodationRating(Guid id);
    Task<PagedResult<RatingResponse>> GetRatings(GetAccommodationRatingsRequest request);
    Task<GetRatingResponse> GetRating(Guid id);
}