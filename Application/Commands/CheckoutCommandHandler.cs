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

        public async Task<CheckoutResult> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                return CheckoutResult.BAD_REQUEST;
            }

            var idempotentRequest = new IdempotentRequest() { Key = request.IdempotencyKey, StatusType = OrderStatusType.CREATED };

            try
            {
                await _idempotentRequestDAO.InsertIdempotentRequest(idempotentRequest);
            }
            catch (UniqueConstraintException)
            {
                var db_idempotentRequest = await _idempotentRequestDAO.LoadByKey(request.IdempotencyKey);

                if (db_idempotentRequest != null)
                {
                    return GetCheckoutResultByOrderStatusType(db_idempotentRequest.StatusType);
                }

                _logger.LogError("Could not find idempotentRequest after UniqueConstraintException.");
                return CheckoutResult.ERROR;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CheckoutResult.ERROR;
            }

            await Task.Delay(5000);

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
}