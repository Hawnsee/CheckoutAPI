using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using CheckoutAPI.Domain;
using CheckoutAPI.DB;
using Microsoft.EntityFrameworkCore;
using EntityFramework.Exceptions.SqlServer;
using EntityFramework.Exceptions.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IdempotentRequestDAO>();

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
                                IdempotentRequestDAO idempotentRequestDAO,
                                ILogger<Program> logger
                            ) =>
{
    if (string.IsNullOrWhiteSpace(idempotencyKey))
    {
        return Results.BadRequest("Invalid request");
    }

    var idempotentRequest = new IdempotentRequest() { Key = idempotencyKey, StatusType = OrderStatusType.CREATED };

    try
    {
        await idempotentRequestDAO.InsertIdempotentRequest(idempotentRequest);
    }
    catch (UniqueConstraintException)
    {
        var db_idempotentRequest = await idempotentRequestDAO.LoadByKey(idempotencyKey);

        if (db_idempotentRequest != null)
        {
            return Results.Conflict($"Conflict. Request is {db_idempotentRequest.StatusType}");
        }

        logger.LogError("Could not find idempotentRequest after UniqueConstraintException.");
        return Results.InternalServerError();
    }
    catch (Exception ex)
    {
        logger.LogError(ex.ToString());
        return Results.InternalServerError();
    }

    await Task.Delay(5000);

    idempotentRequest.StatusType = OrderStatusType.COMPLETED;

    try
    {
        await idempotentRequestDAO.InserOrUpdatetIdempotentRequest(idempotentRequest);
    }
    catch (Exception ex)
    {
        logger.LogError(ex.ToString());
        return Results.InternalServerError();
    }

    return Results.Ok("OK");
})
.WithName("Checkout");

app.Run();
