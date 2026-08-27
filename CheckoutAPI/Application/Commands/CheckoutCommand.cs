using MediatR;

namespace CheckoutAPI.Application.Commands
{
    public record CheckoutCommand() : IRequest<CheckoutResult>
    {
        public string Id { get; set; }

        public string Price { get; set; }
    }
}