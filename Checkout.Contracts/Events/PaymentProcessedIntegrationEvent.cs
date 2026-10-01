namespace Checkout.Contracts.Events
{
    public record PaymentProcessedIntegrationEvent(Guid SagaCorrelationId, bool Success);
}