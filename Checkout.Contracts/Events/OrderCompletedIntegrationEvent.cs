namespace Checkout.Contracts.Events;

public record OrderCompletedIntegrationEvent
{
    public OrderCompletedIntegrationEvent(string id, DateTimeOffset createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }

    public string Id { get; }

    public DateTimeOffset CreatedAt { get; }
}
