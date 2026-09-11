using MassTransit;
using Checkout.Contracts.Commands;
using DAL;

namespace CheckoutAPI.Application.Consumers
{
    public class SetOrderPaymentProcessedConsumer : IConsumer<SetOrderPaymentProcessedCommand>
    {
        private readonly ILogger<SetOrderPaymentProcessedConsumer> _logger;
        private readonly CheckoutOrderDAO _checkoutOrderDAO;

        public SetOrderPaymentProcessedConsumer(ILogger<SetOrderPaymentProcessedConsumer> logger, CheckoutOrderDAO checkoutOrderDAO)
        {
            _logger = logger;
            _checkoutOrderDAO = checkoutOrderDAO;
        }

        public async Task Consume(ConsumeContext<SetOrderPaymentProcessedCommand> context)
        {
            var order = await _checkoutOrderDAO.LoadByIdForUpdate(context.Message.OrderId);

            if (order == null)
            {
                _logger.LogWarning("No se encontro la orden con id {Id}", context.Message.OrderId);
                return;
                
            }

            if (order.Status != CheckoutStatusType.CREATED)
            {
                _logger.LogWarning($"Orden con id {order.Id} en estado {order.Status} intenta transicionar a {CheckoutStatusType.PAYMENT_PROCESSED}. "
                + "Este cambio no esta permitido.");
                return;
            }

            order.Status = CheckoutStatusType.PAYMENT_PROCESSED;
            await _checkoutOrderDAO.SaveChangesAsync();
            _logger.LogInformation($"Orden {context.Message.OrderId} transiciona a estado {CheckoutStatusType.PAYMENT_PROCESSED}");
        }
    }
}