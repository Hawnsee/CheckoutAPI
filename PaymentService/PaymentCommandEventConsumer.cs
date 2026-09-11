namespace PaymentService;

using Checkout.Contracts.Commands;
using Checkout.Contracts.Events;
using MassTransit;

public class PaymentCommandEventConsumer : IConsumer<ProcessPaymentCommand>
{
    private readonly ILogger<PaymentCommandEventConsumer> _logger;

    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentCommandEventConsumer(IPublishEndpoint publishEndpoint, ILogger<PaymentCommandEventConsumer> logger)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<ProcessPaymentCommand> context)
    {
        _logger.LogInformation("Procesando pago para SAGA: {Id}", context.Message.Id);
        await Task.Delay(2000);
        bool result = Random.Shared.Next(2) == 0;
        _logger.LogInformation($"Pago completado con resultado: {result}");
        await _publishEndpoint.Publish(new PaymentProcessedIntegrationEvent(context.Message.Id, result));
    }
}