using CheckoutAPI.DB;
using CheckoutAPI.Domain;
using EntityFramework.Exceptions.Common;
using MediatR;

namespace CheckoutAPI.Application.Commands
{
    public abstract class IdentifiedCommandHandler<T, R> : IRequestHandler<IdentifiedCommand<T, R>, R>
    where T : IRequest<R>
    {
        private readonly IMediator _mediator;
        
        private readonly ILogger<IdentifiedCommandHandler<T, R>> _logger;

        private readonly IdempotentRequestDAO _idempotentRequestDAO;

        public IdentifiedCommandHandler(
            IMediator mediator,
            IdempotentRequestDAO idempotentRequestDAO,
            ILogger<IdentifiedCommandHandler<T, R>> logger)
        {
            _mediator = mediator;
            _logger = logger;
            _idempotentRequestDAO = idempotentRequestDAO;
        }

        protected abstract R CreateResultForDuplicateRequest(OrderStatusType status);

        protected abstract R CreateResultForRequestError();

        public async Task<R> Handle(IdentifiedCommand<T, R> request, CancellationToken cancellationToken)
        {
            var idempotentRequest = new IdempotentRequest() { Key = request.Id, StatusType = OrderStatusType.CREATED };

            try
            {
                await _idempotentRequestDAO.InsertIdempotentRequest(idempotentRequest);
            }
            catch (UniqueConstraintException)
            {
                var db_idempotentRequest = await _idempotentRequestDAO.LoadByKey(request.Id);

                if (db_idempotentRequest != null)
                {
                    return CreateResultForDuplicateRequest(db_idempotentRequest.StatusType);
                }

                _logger.LogError("Could not find idempotentRequest after UniqueConstraintException.");
                return CreateResultForRequestError();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return CreateResultForRequestError();
            }

            var result = await _mediator.Send(request.Command, cancellationToken);
            return result;
        }
    }
}