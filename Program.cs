using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/checkout", async ([FromHeader(Name = "Idempotency-Key")] string idempotencyKey) =>
{
    IMemoryCache memoryCache = app.Services.GetRequiredService<IMemoryCache>();

    if (memoryCache.TryGetValue(idempotencyKey, out string? value) && value?.Equals("Idempotency-Key") == true)
    {
        return Results.Conflict("Conflict");
    }

    memoryCache.Set(idempotencyKey, "Idempotency-Key");

    await Task.Delay(2000);
    return Results.Ok("OK");
})
.WithName("Checkout");

app.Run();
