using TradingSystem.Domain.Enums;

namespace TradingSystem.Application.DTOs;

public record OrderResponse(
    Guid Id,
    Guid AssetId,
    string AssetSymbol,
    Guid UserId,
    decimal Price,
    decimal Quantity,
    decimal RemainingQuantity,
    OrderSide Side,
    OrderStatus Status,
    DateTime CreatedAt
);
