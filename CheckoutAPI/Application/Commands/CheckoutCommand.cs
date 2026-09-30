using CheckoutAPI.Application.Entities;
using MediatR;

namespace CheckoutAPI.Application.Commands
{
    public record CheckoutCommand() : IRequest<CheckoutResponse>
    {
        public string Id { get; set; }

        public string Price { get; set; }
    }
}