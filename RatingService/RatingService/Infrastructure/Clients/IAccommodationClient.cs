namespace RatingService.Infrastructure.Clients;

public interface IAccommodationClient
{
    Task<GetAccommodationResponse> GetAccommodationInfo(Guid id, CancellationToken ct = default);

}