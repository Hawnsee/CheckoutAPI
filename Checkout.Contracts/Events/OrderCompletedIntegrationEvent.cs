namespace Checkout.Contracts.Events;

public record OrderCompletedIntegrationEvent
{
    public OrderCompletedIntegrationEvent(string id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }

    public string Id { get; }

    public DateTime CreatedAt { get; }
}
