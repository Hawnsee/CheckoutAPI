namespace Checkout.Contracts.Events;

public record OrderCreatedIntegrationEvent
{
    public OrderCreatedIntegrationEvent(Guid sagaCorrelationId, string orderId, DateTimeOffset createdAt)
    {
        SagaCorrelationId = sagaCorrelationId;
        OrderId = orderId;
        CreatedAt = createdAt;
    }

    public Guid SagaCorrelationId { get; }

    public DateTimeOffset CreatedAt { get; }

    public string OrderId { get; }
}