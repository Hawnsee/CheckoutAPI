using Checkout.Contracts.Events;
using CheckoutAPI.Application.Entities;
using DAL;
using MassTransit;
using MediatR;
using static MassTransit.ValidationResultExtensions;

namespace CheckoutAPI.Application.Commands
{
    public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResponse>
    {

        private readonly ILogger<CheckoutCommandHandler> _logger;

        private readonly IPublishEndpoint _publishEndpoint;

        private readonly CheckoutOrderDAO _checkoutOrderDAO;

        public CheckoutCommandHandler(
                ILogger<CheckoutCommandHandler> logger,
                IPublishEndpoint publishEndpoint,
                CheckoutOrderDAO checkoutOrderDAO
            )
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _checkoutOrderDAO = checkoutOrderDAO;
        }

        public async Task<CheckoutResponse> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            var order = new CheckoutOrder() { Id = Guid.NewGuid().ToString(), Price = request.Price, Status = CheckoutStatusType.CREATED };

            await _checkoutOrderDAO.InsertCheckoutOrder_NoCommit(order);

            await _publishEndpoint.Publish(new OrderCreatedIntegrationEvent(Guid.NewGuid(), order.Id, DateTimeOffset.UtcNow));

            await _checkoutOrderDAO.SaveChangesAsync();

            _logger.LogInformation($"Order creado. {order.ToString()}");

            return new CheckoutResponse(CheckoutResult.PROCESSED, order.Id);
        }
    }

    public class CheckoutIdentifiedCommandHandler : IdentifiedCommandHandler<CheckoutCommand, CheckoutResponse>
    {
        public CheckoutIdentifiedCommandHandler(
                                            IMediator mediator,
                                            IdempotentRequestDAO idempotentRequestDAO,
                                            ILogger<IdentifiedCommandHandler<CheckoutCommand, CheckoutResponse>> logger
                                        ) : base(mediator, idempotentRequestDAO, logger)
        {

        }

        private CheckoutResult GetCheckoutResultByOrderStatusType(OrderStatusType orderStatusType)
        {
            switch (orderStatusType)
            {
                case OrderStatusType.COMPLETED:
                    return CheckoutResult.PROCESSED;
                case OrderStatusType.CREATED:
                    return CheckoutResult.DUPLICATED;
                default:
                    return CheckoutResult.ERROR;
            }
        }

        protected override CheckoutResponse CreateResultForDuplicateRequest(OrderStatusType status)
        {
            return new CheckoutResponse(GetCheckoutResultByOrderStatusType(status), "");
        }

        protected override CheckoutResponse CreateResultForRequestError()
        {
            return new CheckoutResponse(CheckoutResult.ERROR, "");
        }

        protected override OrderStatusType GetOrderStatusType(CheckoutResponse result)
        {
            switch (result.CheckoutResult)
            {
                case CheckoutResult.PROCESSED:
                    return OrderStatusType.COMPLETED;
                default:
                    return OrderStatusType.FAILED;
            }
        }
    }
}