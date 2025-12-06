using RatingService.Common.Exceptions;
using RatingService.Domain;
using RatingService.Domain.DTOs;
using RatingService.Repositories.Interfaces;
using RatingService.Services.Interfaces;

namespace RatingService.Services;

public class HostRatingService(
    IRepository<HostRating> hostRatingRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IHostRatingService
{
    public async Task CreateOrUpdateHostRating(HostRatingRequest request)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            throw new UnauthorizedAccessException("You don't have access to this action.");

        HostRating? hostRating;

        if (request.Id.HasValue)
        {
            hostRating = await hostRatingRepository.GetByIdAsync(request.Id.Value)
                         ?? throw new NotFoundException("Rating not found.");

            hostRating.Update(request,
                currentUserService.FirstName,
                currentUserService.LastName,
                currentUserService.Username);
        }
        else
        {
            hostRating = HostRating.Create(
                request,
                userId.Value,
                currentUserService.FirstName,
                currentUserService.LastName,
                currentUserService.Username);

            await hostRatingRepository.AddAsync(hostRating);
        }

        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteHostRating(Guid id)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
            throw new UnauthorizedAccessException("You don't have access to this action.");

        var rating = await hostRatingRepository.GetByIdAsync(id)
                     ?? throw new NotFoundException("Rating not found");

        if (rating.GuestId != userId.Value)
            throw new UnauthorizedAccessException("You don't have access to this action.");

        hostRatingRepository.Remove(rating);
        await unitOfWork.SaveChangesAsync();
    }
}