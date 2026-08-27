namespace Checkout.Contracts.Events
{
    public record PaymentProcessedIntegrationEvent(Guid Id, bool Success);
}