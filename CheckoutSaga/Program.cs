using MassTransit;
using DAL;
using Microsoft.EntityFrameworkCore;
using CheckoutSaga;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<CheckoutStateMachine, CheckoutState>()
    .EntityFrameworkRepository(r =>
    {
       r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
       r.ExistingDbContext<ApplicationDBContext>();
    });

    x.AddEntityFrameworkOutbox<ApplicationDBContext>(o =>
    {
        o.UseSqlServer();
    });

    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        cfg.UseEntityFrameworkOutbox<ApplicationDBContext>(context, options =>
        {
            options.MessageDeliveryLimit = 100;
            options.MessageDeliveryTimeout = TimeSpan.FromSeconds(45);
        });
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = context.GetRequiredService<IConfiguration>()["rabbitMqHost"];
        cfg.Host(host);

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
