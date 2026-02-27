namespace TradingSystem.Application.DTOs;

public record PositionResponse(
    Guid AssetId,
    string AssetSymbol,
    decimal NetQuantity,
    int TotalBuys,
    int TotalSells
);
