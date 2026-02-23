using TradingSystem.Domain.Entities;

namespace TradingSystem.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetActiveOrdersByAssetAsync(Guid assetId);
    Task<IEnumerable<Order>> GetOrdersByUserAsync(Guid userId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task UpdateRangeAsync(IEnumerable<Order> orders);
}
