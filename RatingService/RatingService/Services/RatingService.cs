using RatingService.Common.Exceptions;
using RatingService.Domain;
using RatingService.Domain.DTOs;
using RatingService.Repositories.Interfaces;
using RatingService.Services.Interfaces;

namespace RatingService.Services;

public class RatingService(
    IRepository<HostRating> hostRatingRepository,
    IRepository<AccommodationRating> accommodationRatingRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRatingService
{
    public async Task CreateOrUpdateHostRating(HostRatingRequest request)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("You don't have access to this action.");
        }

        HostRating? hostRating;
        if (request.Id.HasValue)
        {
            hostRating = await hostRatingRepository
                .GetByIdAsync(request.Id.Value);
            if (hostRating == null)
            {
                throw new NotFoundException("Rating not found.");
            }

            hostRating.Update(request, currentUserService.FirstName, currentUserService.LastName,
                currentUserService.Username);
        }
        else
        {
            hostRating = HostRating.Create(request, userId.Value, currentUserService.FirstName,
                currentUserService.LastName, currentUserService.Username);

            await hostRatingRepository.AddAsync(hostRating);
        }

        await unitOfWork.SaveChangesAsync();
    }

    public async Task CreateOrUpdateAccommodationRating(AccommodationRatingRequest request)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("You don't have access to this action.");
        }

        AccommodationRating? accommodationRating;
        if (request.Id.HasValue)
        {
            accommodationRating = await accommodationRatingRepository
                .GetByIdAsync(request.Id.Value);
            if (accommodationRating == null)
            {
                throw new NotFoundException("Rating not found.");
            }

            accommodationRating.Update(request, currentUserService.FirstName, currentUserService.LastName,
                currentUserService.Username);
        }
        else
        {
            accommodationRating = AccommodationRating.Create(request, userId.Value, currentUserService.FirstName,
                currentUserService.LastName, currentUserService.Username);

            await accommodationRatingRepository.AddAsync(accommodationRating);
        }

        await unitOfWork.SaveChangesAsync();
    }
}