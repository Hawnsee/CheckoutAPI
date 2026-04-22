using MediatR;

namespace CheckoutAPI.Application.Commands
{
    public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand>
    {
        public Task Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}