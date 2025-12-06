using RatingService.Domain.DTOs;

namespace RatingService.Services.Interfaces;

public interface IAccommodationRatingService
{
    Task CreateOrUpdateAccommodationRating(AccommodationRatingRequest request);
    Task DeleteAccommodationRating(Guid id);
}