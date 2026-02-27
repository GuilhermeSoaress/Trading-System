using TradingSystem.Domain.Enums;

namespace TradingSystem.Application.DTOs;

public record CreateOrderRequest(
    Guid AssetId,
    Guid UserId,
    decimal Price,
    decimal Quantity,
    OrderSide Side
);
