using System.Collections.Concurrent;
using TradingSystem.Domain.Entities;

namespace TradingSystem.Domain.Services;

/// <summary>
/// Central matching engine that manages order books for all assets.
/// Thread-safe: each OrderBook handles its own locking, and the ConcurrentDictionary
/// provides thread-safe access to books.
/// </summary>
public class MatchingEngine
{
    private readonly ConcurrentDictionary<Guid, OrderBook> _books = new();

    /// <summary>
    /// Processes an incoming order: routes it to the correct asset's OrderBook
    /// and returns any trades generated.
    /// </summary>
    public List<Trade> ProcessOrder(Order order)
    {
        var book = _books.GetOrAdd(order.AssetId, assetId => new OrderBook(assetId));
        return book.AddOrder(order);
    }

    /// <summary>
    /// Cancels an order across all books. Returns true if found and removed.
    /// </summary>
    public bool CancelOrder(Guid orderId, Guid assetId)
    {
        if (_books.TryGetValue(assetId, out var book))
        {
            return book.CancelOrder(orderId);
        }

        return false;
    }

    /// <summary>
    /// Gets the order book for a specific asset.
    /// </summary>
    public OrderBook? GetOrderBook(Guid assetId)
    {
        _books.TryGetValue(assetId, out var book);
        return book;
    }

    /// <summary>
    /// Gets all asset IDs that have active order books.
    /// </summary>
    public IEnumerable<Guid> GetActiveAssetIds() => _books.Keys;
}
