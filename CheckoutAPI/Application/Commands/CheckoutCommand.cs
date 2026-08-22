using MediatR;

namespace CheckoutAPI.Application.Commands
{
    public record CheckoutCommand() : IRequest<CheckoutResult>
    {

    }
}