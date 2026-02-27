namespace TradingSystem.Application.DTOs;

public record CreateAssetRequest(
    string Symbol,
    string Name
);
