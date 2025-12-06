using RatingService.Domain.DTOs;
using RatingService.Services.Interfaces;

namespace RatingService.Services;

public class RatingService : IRatingService
{
    public Task CreateOrUpdateHostRating(HostRatingRequest request)
    {
        throw new NotImplementedException();
    }

    public Task CreateOrUpdateAccommodationRating(AccommodationRatingRequest request)
    {
        throw new NotImplementedException();
    }
}