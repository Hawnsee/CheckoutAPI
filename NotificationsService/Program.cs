using NotificationsService;
using MassTransit;
using DAL;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCompletedEventConsumer>();
    x.AddConsumer<OrderCreatedEventConsumer>();

    var useOutbox = builder.Configuration.GetValue("UseTransactionalOutbox", true);

    if (useOutbox)
    {
        x.AddEntityFrameworkOutbox<ApplicationDBContext>(o =>
        {
            o.UseSqlServer();
            o.UseBusOutbox();
        });

        x.AddConfigureEndpointsCallback((context, name, endpointConfigurator) =>
        {
            endpointConfigurator.UseEntityFrameworkOutbox<ApplicationDBContext>(context);
        });
    }

    // A Transport
    x.UsingRabbitMq((context, cfg) =>
    {
        var host = context.GetRequiredService<IConfiguration>()["rabbitMqHost"];
        cfg.Host(host);

        cfg.PrefetchCount = 20;
        cfg.UseConcurrencyLimit(5);

        cfg.UseMessageRetry(r => r.Exponential(
            5,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(120),
            TimeSpan.FromSeconds(2)
        ));

        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromSeconds(30);
            cb.TripThreshold = 15;
            cb.ActiveThreshold = 10;
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();

