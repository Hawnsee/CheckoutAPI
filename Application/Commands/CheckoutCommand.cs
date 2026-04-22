using MediatR;

namespace CheckoutAPI.Application.Commands
{
    public record CheckoutCommand(string IdempotencyKey) : IRequest<IResult>
    {
        
    }
}