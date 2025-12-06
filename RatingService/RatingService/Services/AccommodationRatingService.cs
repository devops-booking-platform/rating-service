using RatingService.Common.Exceptions;
using RatingService.Domain;
using RatingService.Domain.DTOs;
using RatingService.Repositories.Interfaces;
using RatingService.Services.Interfaces;

namespace RatingService.Services;

public class AccommodationRatingService(
    IRepository<AccommodationRating> accommodationRatingRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IAccommodationRatingService
{
    public async Task CreateOrUpdateAccommodationRating(AccommodationRatingRequest request)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            throw new UnauthorizedAccessException("You don't have access to this action.");

        AccommodationRating? rating;

        if (request.Id.HasValue)
        {
            rating = await accommodationRatingRepository.GetByIdAsync(request.Id.Value)
                     ?? throw new NotFoundException("Rating not found.");

            rating.Update(request,
                currentUserService.FirstName,
                currentUserService.LastName,
                currentUserService.Username);
        }
        else
        {
            rating = AccommodationRating.Create(
                request,
                userId.Value,
                currentUserService.FirstName,
                currentUserService.LastName,
                currentUserService.Username);

            await accommodationRatingRepository.AddAsync(rating);
        }

        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAccommodationRating(Guid id)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            throw new UnauthorizedAccessException("You don't have access to this action.");

        var rating = await accommodationRatingRepository.GetByIdAsync(id)
                     ?? throw new NotFoundException("Rating not found");

        if (rating.GuestId != userId.Value)
            throw new UnauthorizedAccessException("You don't have access to this action.");

        accommodationRatingRepository.Remove(rating);
        await unitOfWork.SaveChangesAsync();
    }
}