using TradingSystem.Application.DTOs;

namespace TradingSystem.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> PlaceOrderAsync(CreateOrderRequest request);
    Task<OrderResponse?> GetOrderByIdAsync(Guid id);
    Task<IEnumerable<OrderResponse>> GetOrdersByUserAsync(Guid userId);
    Task<bool> CancelOrderAsync(Guid orderId, Guid assetId);
    Task<OrderBookResponse> GetOrderBookAsync(Guid assetId);
}
