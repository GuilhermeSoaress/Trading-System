using TradingSystem.Application.DTOs;
using TradingSystem.Domain.Entities;

namespace TradingSystem.Application.Mappers;

public static class OrderMapper
{
    public static OrderResponse ToResponse(this Order order)
    {
        return new OrderResponse(
            Id: order.Id,
            AssetId: order.AssetId,
            AssetSymbol: order.Asset?.Symbol ?? string.Empty,
            UserId: order.UserId,
            Price: order.Price,
            Quantity: order.Quantity,
            RemainingQuantity: order.RemainingQuantity,
            Side: order.Side,
            Status: order.Status,
            CreatedAt: order.CreatedAt
        );
    }
}
