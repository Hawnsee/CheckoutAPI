using Microsoft.AspNetCore.Mvc;
using DAL;
using Microsoft.EntityFrameworkCore;
using EntityFramework.Exceptions.SqlServer;
using MediatR;
using CheckoutAPI.Application.Commands;
using MassTransit;
using CheckoutAPI.Entities;
using CheckoutAPI.Application.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IdempotentRequestDAO>();
builder.Services.AddScoped<CheckoutOrderDAO>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CancelOrderConsumer>();
    x.AddConsumer<SetOrderPaymentProcessedConsumer>();

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

    x.AddEntityFrameworkOutbox<ApplicationDBContext>(o =>
    {
        o.UseSqlServer();
        o.UseBusOutbox();
    });
});

var httpClientBuilder = builder.Services.AddHttpClient("PaymentClient");
httpClientBuilder.AddStandardResilienceHandler();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(connectionString).UseExceptionProcessor());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/checkout/status/{id}", async ([FromRoute] string id, CheckoutOrderDAO checkoutOrderDAO) =>
{
    var record = await checkoutOrderDAO.LoadById(id);
    
    if (record == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(record.Status.ToString());

}).WithName("GetCheckoutStatus");

app.MapPost("/api/checkout", async (
                                [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
                                CheckoutRecord checkoutStruct,
                                IMediator _mediator
                            ) =>
{

    if (string.IsNullOrWhiteSpace(idempotencyKey)
    || string.IsNullOrWhiteSpace(checkoutStruct?.price))
    {
        return Results.BadRequest();
    }

    CheckoutResult result = await _mediator.Send(new IdentifiedCommand<CheckoutCommand, CheckoutResult>(
        new CheckoutCommand() { Id = idempotencyKey, Price = checkoutStruct.price },
        idempotencyKey
    ));

    switch (result)
    {
        case CheckoutResult.PROCESSED:
            return Results.Accepted("/api/checkout/status/{id}", idempotencyKey);
        case CheckoutResult.DUPLICATED:
            return Results.Conflict();
        case CheckoutResult.BAD_REQUEST:
            return Results.BadRequest();
        default:
            return Results.InternalServerError();
    }
})
.WithName("Checkout");

app.Run();
