namespace Checkout.Contracts.Events;

public record OrderCreatedIntegrationEvent
{
    public OrderCreatedIntegrationEvent(Guid id, string orderId, DateTime createdAt)
    {
        Id = id;
        OrderId = orderId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }

    public DateTime CreatedAt { get; }

    public string OrderId { get; }
}