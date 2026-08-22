namespace NotificationsService;

using System.Threading.Tasks;
using Checkout.Contracts.Events;
using MassTransit;

public class OrderCompletedEventConsumer : IConsumer<OrderCompletedIntegrationEvent>
{
    private readonly ILogger<OrderCompletedEventConsumer> _logger;

    public OrderCompletedEventConsumer(ILogger<OrderCompletedEventConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedIntegrationEvent> context)
    {
        _logger.LogWarning("Ejecutando intento de consumo para la orden: {Id} a las {Time}", context.Message.Id, DateTime.UtcNow.ToString("HH:mm:ss"));
        await Task.Delay(2000);
        _logger.LogInformation("Orden procesada para notificaciones: {Id} a las {Time}", context.Message.Id, DateTime.UtcNow.ToString("HH:mm:ss"));
    }
}