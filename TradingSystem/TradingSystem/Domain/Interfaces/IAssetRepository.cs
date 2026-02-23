using TradingSystem.Domain.Entities;

namespace TradingSystem.Domain.Interfaces;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(Guid id);
    Task<Asset?> GetBySymbolAsync(string symbol);
    Task<IEnumerable<Asset>> GetAllAsync();
    Task AddAsync(Asset asset);
    Task<bool> ExistsAsync(Guid id);
}
