using Microsoft.AspNetCore.Mvc;
using DAL;
using Microsoft.EntityFrameworkCore;
using EntityFramework.Exceptions.SqlServer;
using MediatR;
using CheckoutAPI.Application.Commands;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IdempotentRequestDAO>();

builder.Services.AddMassTransit(x =>
{
    // A Transport
    x.UsingRabbitMq((context, cfg) =>
    {
        var host = context.GetRequiredService<IConfiguration>()["rabbitMqHost"];
        cfg.Host(host);
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

app.MapGet("/api/idempotencyRequest/{id}", async (
                                                [FromRoute] string id,
                                                IdempotentRequestDAO idempotentRequestDAO
                                            ) =>
{

    var record = await idempotentRequestDAO.LoadByKey(id);
    return Results.Ok(record?.ToString());

}).WithName("GetIdempotencyRequest");

app.MapPost("/api/checkout", async (
                                [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
                                IMediator _mediator
                            ) =>
{

    if (string.IsNullOrWhiteSpace(idempotencyKey))
    {
        return Results.BadRequest();
    }

    CheckoutResult result = await _mediator.Send(new IdentifiedCommand<CheckoutCommand, CheckoutResult>(
        new CheckoutCommand(),
        idempotencyKey
    ));

    switch (result)
    {
        case CheckoutResult.COMPLETED:
            return Results.Ok();
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
