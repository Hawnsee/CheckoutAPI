namespace CheckoutAPI.Application.Consumers;

using Checkout.Contracts.Commands;
using MassTransit;
using DAL;

public class CancelOrderConsumer : IConsumer<CancelOrderCommand>
{
    private readonly ILogger<CancelOrderConsumer> _logger;
    private readonly CheckoutOrderDAO _checkoutOrderDAO;

    public CancelOrderConsumer(ILogger<CancelOrderConsumer> logger, CheckoutOrderDAO checkoutOrderDAO)
    {
        _logger = logger;
        _checkoutOrderDAO = checkoutOrderDAO;
    }

    public async Task Consume(ConsumeContext<CancelOrderCommand> context)
    {
        var order = await _checkoutOrderDAO.LoadByIdForUpdate(context.Message.OrderId);
        if (order != null)
        {
            order.Status = CheckoutStatusType.CANCELLED;
            await _checkoutOrderDAO.SaveChangesAsync();
            _logger.LogInformation("Orden cancelada: {Id}", context.Message.OrderId);
        }
        else
        {
            _logger.LogWarning("No se encontro la orden con id {Id}", context.Message.OrderId);
        }
    }
}
