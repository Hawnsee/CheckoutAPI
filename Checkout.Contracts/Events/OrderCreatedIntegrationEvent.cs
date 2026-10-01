namespace Checkout.Contracts.Events;

public record OrderCreatedIntegrationEvent
{
    public OrderCreatedIntegrationEvent(Guid id, string orderId, DateTime createdAt)
    {
        SagaCorrelationId = id;
        OrderId = orderId;
        CreatedAt = createdAt;
    }

    public Guid SagaCorrelationId { get; }

    public DateTime CreatedAt { get; }

    public string OrderId { get; }
}