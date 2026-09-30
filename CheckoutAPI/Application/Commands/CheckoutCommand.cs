using CheckoutAPI.Application.Entities;
using MediatR;

namespace CheckoutAPI.Application.Commands
{
    public record CheckoutCommand() : IRequest<CheckoutResponse>
    {
        public string Price { get; set; }
    }
}