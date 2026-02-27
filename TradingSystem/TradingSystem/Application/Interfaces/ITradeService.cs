using TradingSystem.Application.DTOs;

namespace TradingSystem.Application.Interfaces;

public interface ITradeService
{
    Task<IEnumerable<TradeResponse>> GetTradesByAssetAsync(Guid assetId);
    Task<IEnumerable<TradeResponse>> GetTradesByUserAsync(Guid userId);
    Task<IEnumerable<TradeResponse>> GetRecentTradesAsync(int count = 50);
    Task<IEnumerable<PositionResponse>> GetPositionsByUserAsync(Guid userId);
}
