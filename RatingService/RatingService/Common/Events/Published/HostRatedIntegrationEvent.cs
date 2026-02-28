namespace RatingService.Common.Events.Published
{
    public record HostRatedIntegrationEvent(Guid HostId, string GuestUsername, int Rating) : IIntegrationEvent;
}
