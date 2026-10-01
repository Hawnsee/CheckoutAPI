namespace Checkout.Contracts.Events;

public record OrderCreatedIntegrationEvent
{
    public OrderCreatedIntegrationEvent(Guid sagaCorrelationId, string orderId, DateTime createdAt)
    {
        SagaCorrelationId = sagaCorrelationId;
        OrderId = orderId;
        CreatedAt = createdAt;
    }

    public Guid SagaCorrelationId { get; }

    public DateTime CreatedAt { get; }

    public string OrderId { get; }
}