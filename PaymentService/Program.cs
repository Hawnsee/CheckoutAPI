using MassTransit;
using PaymentService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentCommandEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = context.GetRequiredService<IConfiguration>()["rabbitMqHost"];
        cfg.Host(host);
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
