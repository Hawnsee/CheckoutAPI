namespace Checkout.Contracts.Commands
{
    public record ProcessPaymentCommand
    {
        public Guid SagaCorrelationId { get; set; }

        public string OrderId { get; set; }
    }
}