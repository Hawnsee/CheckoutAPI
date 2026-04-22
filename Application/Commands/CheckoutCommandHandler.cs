using CheckoutAPI.DB;
using CheckoutAPI.Domain;
using EntityFramework.Exceptions.Common;
using MediatR;

namespace CheckoutAPI.Application.Commands
{
    public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, IResult>
    {
        public IdempotentRequestDAO _idempotentRequestDAO { get; set; }

        public ILogger<CheckoutCommandHandler> _logger { get; set; }

        public CheckoutCommandHandler(IdempotentRequestDAO idempotentRequestDAO, ILogger<CheckoutCommandHandler> logger)
        {
            _idempotentRequestDAO = idempotentRequestDAO;
            _logger = logger;
        }

        public async Task<IResult> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                return Results.BadRequest("Invalid request");
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
                    return Results.Conflict($"Conflict. Request is {db_idempotentRequest.StatusType}");
                }

                _logger.LogError("Could not find idempotentRequest after UniqueConstraintException.");
                return Results.InternalServerError();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Results.InternalServerError();
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
                return Results.InternalServerError();
            }

            return Results.Ok("OK");
        }
    }
}