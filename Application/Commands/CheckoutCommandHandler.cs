using CheckoutAPI.DB;
using CheckoutAPI.Domain;
using MediatR;

namespace CheckoutAPI.Application.Commands
{
    public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResult>
    {
        private readonly IdempotentRequestDAO _idempotentRequestDAO;

        private readonly ILogger<CheckoutCommandHandler> _logger;

        private readonly HttpClient _httpClient;

        public CheckoutCommandHandler(
                IdempotentRequestDAO idempotentRequestDAO, 
                ILogger<CheckoutCommandHandler> logger,
                IHttpClientFactory httpClientFactory
            )
        {
            _idempotentRequestDAO = idempotentRequestDAO;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("PaymentClient");
        }

        public async Task<CheckoutResult> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando llamada al servicio de pagos externo...");

            // Usamos httpstat.us, un servicio público diseñado para forzar códigos de respuesta HTTP específicos. 
            // Un error 500 simula que la pasarela de pago externa está totalmente colapsada.

            try
            {
                var response = await _httpClient.GetAsync("https://httpstat.us/500", cancellationToken);

                _logger.LogInformation("Llamada finalizada con estado: {StatusCode}", response.StatusCode);

                return CheckoutResult.COMPLETED;
            }
            catch (HttpRequestException)
            {
                _logger.LogError("El servicio de pagos ha fallado tras todos los reintentos.");
                return CheckoutResult.ERROR;
            }
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