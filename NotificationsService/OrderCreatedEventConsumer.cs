namespace NotificationsService;

using Checkout.Contracts.Events;
using MassTransit;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedIntegrationEvent>
{
    private readonly ILogger<OrderCreatedEventConsumer> _logger;

    public OrderCreatedEventConsumer(ILogger<OrderCreatedEventConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("Orden creada para SAGA: {Id}", context.Message.Id);
    }
}