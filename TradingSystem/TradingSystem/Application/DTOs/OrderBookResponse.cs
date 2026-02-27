namespace TradingSystem.Application.DTOs;

public record OrderBookResponse(
    Guid AssetId,
    decimal? BestBid,
    decimal? BestAsk,
    decimal? Spread,
    IReadOnlyList<OrderBookLevel> Bids,
    IReadOnlyList<OrderBookLevel> Asks
);

public record OrderBookLevel(
    decimal Price,
    decimal TotalQuantity
);
