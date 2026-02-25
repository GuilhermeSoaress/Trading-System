using TradingSystem.Application.DTOs;
using TradingSystem.Domain.Entities;

namespace TradingSystem.Application.Mappers;

public static class TradeMapper
{
    public static TradeResponse ToResponse(this Trade trade)
    {
        return new TradeResponse(
            Id: trade.Id,
            AssetId: trade.AssetId,
            AssetSymbol: trade.Asset?.Symbol,
            BuyOrderId: trade.BuyOrderId,
            SellOrderId: trade.SellOrderId,
            Price: trade.Price,
            Quantity: trade.Quantity,
            ExecutedAt: trade.ExecutedAt
        );
    }
}
