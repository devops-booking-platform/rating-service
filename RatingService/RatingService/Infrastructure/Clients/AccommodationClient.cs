using System.Net;
using RatingService.Common.Exceptions;

namespace RatingService.Infrastructure.Clients;

public class AccommodationClient(HttpClient http) : IAccommodationClient
{
    public async Task<GetAccommodationResponse> GetAccommodationInfo(Guid id, CancellationToken ct = default)
    {
        var url = $"/api/accommodations/{id}";
        using var resp = await http.GetAsync(url, ct);
        if (resp.IsSuccessStatusCode)
        {
            var dto = await resp.Content.ReadFromJsonAsync<GetAccommodationResponse>(cancellationToken: ct);
            return dto ?? throw new ExternalServiceException("AccommodationService returned empty response.");
        }
        if (resp.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException("Accommodation not found.");
        
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
            throw new ExternalServiceException("AccommodationService unauthorized.");

        if (resp.StatusCode == HttpStatusCode.Forbidden)
            throw new ExternalServiceException("AccommodationService forbidden.");
        
        throw new ExternalServiceException($"AccommodationService error ({(int)resp.StatusCode}).");
    }
}