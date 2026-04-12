using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using CheoutAPI.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ConcurrentDictionary<string, OrderStatusType>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/checkout", async (
                                [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
                                ConcurrentDictionary<string, OrderStatusType> requests
                            ) =>
{
    if (idempotencyKey.Trim().Equals(string.Empty))
    {
        return Results.BadRequest("Invalid request");
    }

    if (requests.TryGetValue(idempotencyKey, out OrderStatusType status))
    {
        return Results.Conflict($"Conflict. Request is {status}");
    }


    await Task.Delay(2000);
    return Results.Ok("OK");
})
.WithName("Checkout");

app.Run();
