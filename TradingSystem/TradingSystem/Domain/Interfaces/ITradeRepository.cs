using TradingSystem.Domain.Entities;

namespace TradingSystem.Domain.Interfaces;

public interface ITradeRepository
{
    Task<IEnumerable<Trade>> GetByAssetAsync(Guid assetId);
    Task<IEnumerable<Trade>> GetByUserAsync(Guid userId);
    Task<IEnumerable<Trade>> GetRecentAsync(int count = 50);
    Task AddAsync(Trade trade);
    Task AddRangeAsync(IEnumerable<Trade> trades);
}
