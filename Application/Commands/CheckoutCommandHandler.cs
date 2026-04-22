using CheckoutAPI.DB;
using CheckoutAPI.Domain;
using EntityFramework.Exceptions.Common;
using MediatR;

namespace CheckoutAPI.Application.Commands
{
    public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResult>
    {
        private readonly IdempotentRequestDAO _idempotentRequestDAO;

        private readonly ILogger<CheckoutCommandHandler> _logger;

        public CheckoutCommandHandler(IdempotentRequestDAO idempotentRequestDAO, ILogger<CheckoutCommandHandler> logger)
        {
            _idempotentRequestDAO = idempotentRequestDAO;
            _logger = logger;
        }

        public async Task<CheckoutResult> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            await Task.Delay(5000);

            var idempotentRequest = await _idempotentRequestDAO.LoadByKey(request.IdempotencyKey);

            if (idempotentRequest == null)
            {
                return CheckoutResult.ERROR;
            }

            idempotentRequest.StatusType = OrderStatusType.COMPLETED;

            try
            {
                await _idempotentRequestDAO.InserOrUpdatetIdempotentRequest(idempotentRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CheckoutResult.ERROR;
            }

            return CheckoutResult.COMPLETED;
        }
    }

    public class CheckoutIdentifiedCommandHandler : IdentifiedCommandHandler<CheckoutCommand, CheckoutResult>
    {
        public CheckoutIdentifiedCommandHandler(
                                            IMediator mediator, 
                                            IdempotentRequestDAO idempotentRequestDAO, 
                                            ILogger<IdentifiedCommandHandler<CheckoutCommand, CheckoutResult>> logger
                                        ) : base(mediator, idempotentRequestDAO, logger)
        {
            
        }

        private CheckoutResult GetCheckoutResultByOrderStatusType(OrderStatusType orderStatusType)
        {
            switch (orderStatusType)
            {
                case OrderStatusType.COMPLETED:
                    return CheckoutResult.COMPLETED;
                case OrderStatusType.CREATED:
                    return CheckoutResult.DUPLICATED;
                default:
                    return CheckoutResult.ERROR;
            }
        }

        protected override CheckoutResult CreateResultForDuplicateRequest(OrderStatusType status)
        {
            return GetCheckoutResultByOrderStatusType(status);
        }

        protected override CheckoutResult CreateResultForRequestError()
        {
            return CheckoutResult.ERROR;
        }
    }
}