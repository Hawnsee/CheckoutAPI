namespace Checkout.Contracts.Events
{
    public record PaymentProcessedIntegrationEvent(Guid Id, string OrderId, bool Success);
}