using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using CheckoutAPI.Domain;
using CheckoutAPI.DB;
using Microsoft.EntityFrameworkCore;
using EntityFramework.Exceptions.SqlServer;
using EntityFramework.Exceptions.Common;
using MediatR;
using CheckoutAPI.Application.Commands;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IdempotentRequestDAO>();

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
                                            ) => {

    var record = await idempotentRequestDAO.LoadByKey(id);
    return Results.Ok(record?.ToString());

}).WithName("GetIdempotencyRequest");

app.MapPost("/api/checkout", async (
                                [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
                                IMediator _mediator
                            ) =>
{
    CheckoutResult result = await _mediator.Send(new CheckoutCommand(idempotencyKey));
    
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
