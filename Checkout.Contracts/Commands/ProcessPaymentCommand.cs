namespace Checkout.Contracts.Commands
{
    public record ProcessPaymentCommand
    {
        public Guid Id { get; set; }

        public string OrderId { get; set; }
    }
}