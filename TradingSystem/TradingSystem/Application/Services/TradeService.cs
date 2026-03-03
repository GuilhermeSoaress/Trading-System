using TradingSystem.Application.DTOs;
using TradingSystem.Application.Interfaces;
using TradingSystem.Application.Mappers;
using TradingSystem.Domain.Interfaces;

namespace TradingSystem.Application.Services;

public class TradeService : ITradeService
{
    private readonly ITradeRepository _tradeRepository;
    private readonly IOrderRepository _orderRepository;

    public TradeService(ITradeRepository tradeRepository, IOrderRepository orderRepository)
    {
        _tradeRepository = tradeRepository;
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<TradeResponse>> GetTradesByAssetAsync(Guid assetId)
    {
        var trades = await _tradeRepository.GetByAssetAsync(assetId);
        return trades.Select(t => t.ToResponse());
    }

    public async Task<IEnumerable<TradeResponse>> GetTradesByUserAsync(Guid userId)
    {
        var trades = await _tradeRepository.GetByUserAsync(userId);
        return trades.Select(t => t.ToResponse());
    }

    public async Task<IEnumerable<TradeResponse>> GetRecentTradesAsync(int count = 50)
    {
        var trades = await _tradeRepository.GetRecentAsync(count);
        return trades.Select(t => t.ToResponse());
    }

    public async Task<IEnumerable<PositionResponse>> GetPositionsByUserAsync(Guid userId)
    {
        var trades = await _tradeRepository.GetByUserAsync(userId);
        var orders = await _orderRepository.GetOrdersByUserAsync(userId);

        // Calculate net position per asset
        var positions = new Dictionary<Guid, (string Symbol, decimal Net, int Buys, int Sells)>();

        foreach (var trade in trades)
        {
            if (!positions.ContainsKey(trade.AssetId))
            {
                positions[trade.AssetId] = (trade.Asset?.Symbol ?? trade.AssetId.ToString(), 0, 0, 0);
            }

            var pos = positions[trade.AssetId];

            // Check if user was the buyer or seller
            var buyOrder = orders.FirstOrDefault(o => o.Id == trade.BuyOrderId);
            var sellOrder = orders.FirstOrDefault(o => o.Id == trade.SellOrderId);

            if (buyOrder != null && buyOrder.UserId == userId)
            {
                positions[trade.AssetId] = (pos.Symbol, pos.Net + trade.Quantity, pos.Buys + 1, pos.Sells);
            }

            if (sellOrder != null && sellOrder.UserId == userId)
            {
                positions[trade.AssetId] = (pos.Symbol, pos.Net - trade.Quantity, pos.Buys, pos.Sells + 1);
            }
        }

        return positions.Select(p => new PositionResponse(
            AssetId: p.Key,
            AssetSymbol: p.Value.Symbol,
            NetQuantity: p.Value.Net,
            TotalBuys: p.Value.Buys,
            TotalSells: p.Value.Sells
        ));
    }
}
