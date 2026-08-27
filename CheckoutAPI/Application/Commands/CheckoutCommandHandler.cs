using Checkout.Contracts.Events;
using DAL;
using MassTransit;
using MediatR;

namespace CheckoutAPI.Application.Commands
{
    public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResult>
    {

        private readonly ILogger<CheckoutCommandHandler> _logger;

        private readonly HttpClient _httpClient;

        private readonly IPublishEndpoint _publishEndpoint;

        private readonly CheckoutOrderDAO _checkoutOrderDAO;

        public CheckoutCommandHandler(
                ILogger<CheckoutCommandHandler> logger,
                IHttpClientFactory httpClientFactory,
                IPublishEndpoint publishEndpoint,
                CheckoutOrderDAO checkoutOrderDAO
            )
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("PaymentClient");
            _publishEndpoint = publishEndpoint;
            _checkoutOrderDAO = checkoutOrderDAO;
        }

        public async Task<CheckoutResult> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando llamada al servicio de pagos externo...");

            // Usamos httpstat.us, un servicio público diseñado para forzar códigos de respuesta HTTP específicos. 
            // Un error 500 simula que la pasarela de pago externa está totalmente colapsada.

            try
            {
                // var response = await _httpClient.GetAsync("https://tools-httpstatus.pickup-services.com/200?sleep=5000", cancellationToken);
                // response.EnsureSuccessStatusCode();

                var order = new CheckoutOrder() { Id = request.Id, Price = request.Price, Status = CheckoutStatusType.CREATED };

                await _checkoutOrderDAO.InsertCheckoutOrder_NoCommit(order);

                await _publishEndpoint.Publish(new OrderCreatedIntegrationEvent(Guid.NewGuid(), order.Id, DateTime.Now));

                await _checkoutOrderDAO.SaveChangesAsync();

                _logger.LogInformation($"Order creado. {order.ToString()}");

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

        protected override OrderStatusType GetOrderStatusType(CheckoutResult result)
        {
            switch (result)
            {
                case CheckoutResult.COMPLETED:
                    return OrderStatusType.COMPLETED;
                default:
                    return OrderStatusType.FAILED;
            }
        }
    }
}