namespace TradingSystem.Application.DTOs;

public record AssetResponse(
    Guid Id,
    string Symbol,
    string Name,
    DateTime CreatedAt
);
