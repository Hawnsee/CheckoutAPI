using CheckoutAPI.Application.Entities;
using MediatR;

namespace CheckoutAPI.Application.Commands
{
    public record CheckoutCommand() : IRequest<CheckoutResponse>
    {
        public decimal Price { get; set; }
    }
}