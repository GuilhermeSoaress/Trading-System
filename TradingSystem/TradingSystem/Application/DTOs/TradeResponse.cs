namespace TradingSystem.Application.DTOs;

public record TradeResponse(
    Guid Id,
    Guid AssetId,
    string? AssetSymbol,
    Guid BuyOrderId,
    Guid SellOrderId,
    decimal Price,
    decimal Quantity,
    DateTime ExecutedAt
);
