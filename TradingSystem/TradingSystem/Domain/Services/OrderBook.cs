using TradingSystem.Domain.Entities;
using TradingSystem.Domain.Enums;

namespace TradingSystem.Domain.Services;

/// <summary>
/// In-memory order book for a single asset. Maintains buy (bid) and sell (ask) sides
/// using price-time priority (FIFO). Thread-safe via lock.
/// </summary>
public class OrderBook
{
    private readonly object _lock = new();

    // Bids: highest price first (descending). We use a custom comparer.
    private readonly SortedDictionary<decimal, Queue<Order>> _bids = new(new DescendingComparer());

    // Asks: lowest price first (ascending). Default SortedDictionary order.
    private readonly SortedDictionary<decimal, Queue<Order>> _asks = new();

    public Guid AssetId { get; }

    public OrderBook(Guid assetId)
    {
        AssetId = assetId;
    }

    /// <summary>
    /// Adds an order to the book and attempts to match it against the opposite side.
    /// Returns a list of trades generated from the matching process.
    /// </summary>
    public List<Trade> AddOrder(Order order)
    {
        if (order.AssetId != AssetId)
            throw new ArgumentException($"Order asset {order.AssetId} does not match book asset {AssetId}");

        lock (_lock)
        {
            var trades = Match(order);

            // If the order still has remaining quantity, place it in the book
            if (order.RemainingQuantity > 0 && order.Status != OrderStatus.Cancelled)
            {
                var book = order.Side == OrderSide.Buy ? _bids : _asks;

                if (!book.TryGetValue(order.Price, out var queue))
                {
                    queue = new Queue<Order>();
                    book[order.Price] = queue;
                }

                queue.Enqueue(order);
            }

            return trades;
        }
    }

    /// <summary>
    /// Cancels an order and removes it from the book.
    /// </summary>
    public bool CancelOrder(Guid orderId)
    {
        lock (_lock)
        {
            return RemoveFromBook(orderId, _bids) || RemoveFromBook(orderId, _asks);
        }
    }

    /// <summary>
    /// Returns the best bid (highest buy price) or null if no bids.
    /// </summary>
    public decimal? BestBid
    {
        get
        {
            lock (_lock)
            {
                return GetBestPrice(_bids);
            }
        }
    }

    /// <summary>
    /// Returns the best ask (lowest sell price) or null if no asks.
    /// </summary>
    public decimal? BestAsk
    {
        get
        {
            lock (_lock)
            {
                return GetBestPrice(_asks);
            }
        }
    }

    /// <summary>
    /// Returns a snapshot of current bid levels (price + total quantity).
    /// </summary>
    public IReadOnlyList<(decimal Price, decimal TotalQuantity)> GetBidLevels()
    {
        lock (_lock)
        {
            return GetLevels(_bids);
        }
    }

    /// <summary>
    /// Returns a snapshot of current ask levels (price + total quantity).
    /// </summary>
    public IReadOnlyList<(decimal Price, decimal TotalQuantity)> GetAskLevels()
    {
        lock (_lock)
        {
            return GetLevels(_asks);
        }
    }

    private List<Trade> Match(Order incoming)
    {
        var trades = new List<Trade>();

        // Determine which side to match against
        var oppositeBook = incoming.Side == OrderSide.Buy ? _asks : _bids;

        while (incoming.RemainingQuantity > 0 && oppositeBook.Count > 0)
        {
            var bestPrice = oppositeBook.Keys.First();

            // Check if price is compatible
            if (incoming.Side == OrderSide.Buy && incoming.Price < bestPrice)
                break; // Buy price is lower than lowest ask — no match

            if (incoming.Side == OrderSide.Sell && incoming.Price > bestPrice)
                break; // Sell price is higher than highest bid — no match

            var queue = oppositeBook[bestPrice];

            while (incoming.RemainingQuantity > 0 && queue.Count > 0)
            {
                var resting = queue.Peek();

                // Calculate fill quantity
                var fillQty = Math.Min(incoming.RemainingQuantity, resting.RemainingQuantity);

                // Execute the fill
                incoming.Fill(fillQty);
                resting.Fill(fillQty);

                // Create the trade
                var trade = new Trade
                {
                    Id = Guid.NewGuid(),
                    AssetId = AssetId,
                    BuyOrderId = incoming.Side == OrderSide.Buy ? incoming.Id : resting.Id,
                    SellOrderId = incoming.Side == OrderSide.Sell ? incoming.Id : resting.Id,
                    Price = resting.Price, // Resting order's price (price-time priority)
                    Quantity = fillQty,
                    ExecutedAt = DateTime.UtcNow
                };

                trades.Add(trade);

                // Remove fully filled resting order from queue
                if (resting.RemainingQuantity <= 0)
                {
                    queue.Dequeue();
                }
            }

            // Remove empty price level
            if (queue.Count == 0)
            {
                oppositeBook.Remove(bestPrice);
            }
        }

        return trades;
    }

    private static bool RemoveFromBook(Guid orderId, SortedDictionary<decimal, Queue<Order>> book)
    {
        foreach (var kvp in book)
        {
            var originalCount = kvp.Value.Count;
            var filtered = new Queue<Order>(kvp.Value.Where(o => o.Id != orderId));

            if (filtered.Count < originalCount)
            {
                if (filtered.Count == 0)
                    book.Remove(kvp.Key);
                else
                    book[kvp.Key] = filtered;

                return true;
            }
        }

        return false;
    }

    private static decimal? GetBestPrice(SortedDictionary<decimal, Queue<Order>> book)
    {
        return book.Count > 0 ? book.Keys.First() : null;
    }

    private static List<(decimal Price, decimal TotalQuantity)> GetLevels(SortedDictionary<decimal, Queue<Order>> book)
    {
        return book.Select(kvp => (kvp.Key, kvp.Value.Sum(o => o.RemainingQuantity))).ToList();
    }

    /// <summary>
    /// Comparer for descending order (used for bids — highest price first).
    /// </summary>
    private class DescendingComparer : IComparer<decimal>
    {
        public int Compare(decimal x, decimal y) => y.CompareTo(x);
    }
}
