namespace RatingService.Common.Events.Published
{
    public record AccommodationRatedIntegrationEvent(
    Guid HostId,
    Guid AccommodationId,
    string GuestUsername,
    string AccommodationName,
    int Rating)
    : IIntegrationEvent;
}
