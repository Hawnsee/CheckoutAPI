using CheckoutAPI.Application.Commands;

namespace CheckoutAPI.Application.Entities
{
    public record CheckoutResponse(CheckoutResult CheckoutResult, string OrderId);
}
