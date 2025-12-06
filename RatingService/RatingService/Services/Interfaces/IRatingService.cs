using RatingService.Domain.DTOs;

namespace RatingService.Services.Interfaces;

public interface IRatingService
{
    Task CreateOrUpdateHostRating(HostRatingRequest request);
    Task CreateOrUpdateAccommodationRating(AccommodationRatingRequest request);
}