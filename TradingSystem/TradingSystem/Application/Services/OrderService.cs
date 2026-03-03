using FluentValidation;
using TradingSystem.Application.DTOs;
using TradingSystem.Application.Interfaces;
using TradingSystem.Application.Mappers;
using TradingSystem.Domain.Entities;
using TradingSystem.Domain.Interfaces;
using TradingSystem.Domain.Services;

namespace TradingSystem.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITradeRepository _tradeRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly MatchingEngine _matchingEngine;
    private readonly IValidator<CreateOrderRequest> _validator;
    private readonly ITradeQueuePublisher _tradeQueuePublisher;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        ITradeRepository tradeRepository,
        IAssetRepository assetRepository,
        IUnitOfWork unitOfWork,
        MatchingEngine matchingEngine,
        IValidator<CreateOrderRequest> validator,
        ITradeQueuePublisher tradeQueuePublisher,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _tradeRepository = tradeRepository;
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
        _matchingEngine = matchingEngine;
        _validator = validator;
        _tradeQueuePublisher = tradeQueuePublisher;
        _logger = logger;
    }

    public async Task<OrderResponse> PlaceOrderAsync(CreateOrderRequest request)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Verify asset exists
        var assetExists = await _assetRepository.ExistsAsync(request.AssetId);
        if (!assetExists)
        {
            throw new KeyNotFoundException($"Asset with ID '{request.AssetId}' not found.");
        }

        // Create the domain entity
        var order = new Order
        {
            Id = Guid.NewGuid(),
            AssetId = request.AssetId,
            UserId = request.UserId,
            Price = request.Price,
            Quantity = request.Quantity,
            RemainingQuantity = request.Quantity,
            Side = request.Side,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Processing {Side} order {OrderId} for asset {AssetId}: {Quantity} @ {Price}",
            order.Side, order.Id, order.AssetId, order.Quantity, order.Price);

        // Run through matching engine
        var trades = _matchingEngine.ProcessOrder(order);

        // Persist order
        await _orderRepository.AddAsync(order);

        // Persist trades
        if (trades.Count > 0)
        {
            await _tradeRepository.AddRangeAsync(trades);

            _logger.LogInformation(
                "Order {OrderId} generated {TradeCount} trade(s). Status: {Status}",
                order.Id, trades.Count, order.Status);

            // Update matched resting orders in DB
            foreach (var trade in trades)
            {
                var restingOrderId = order.Side == Domain.Enums.OrderSide.Buy
                    ? trade.SellOrderId
                    : trade.BuyOrderId;

                if (restingOrderId != order.Id)
                {
                    var restingOrder = await _orderRepository.GetByIdAsync(restingOrderId);
                    if (restingOrder != null)
                    {
                        await _orderRepository.UpdateAsync(restingOrder);
                    }
                }
            }
        }

        // Save everything in one transaction
        await _unitOfWork.SaveChangesAsync();

        // Publish trades to RabbitMQ for async processing
        foreach (var trade in trades)
        {
            await _tradeQueuePublisher.PublishTradeForProcessingAsync(trade.Id);
            _logger.LogInformation(
                "Trade {TradeId} published to processing queue (Status: Pending)",
                trade.Id);
        }

        // Load the asset for the response
        var asset = await _assetRepository.GetByIdAsync(order.AssetId);
        order.Asset = asset;

        return order.ToResponse();
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order?.ToResponse();
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersByUserAsync(Guid userId)
    {
        var orders = await _orderRepository.GetOrdersByUserAsync(userId);
        return orders.Select(o => o.ToResponse());
    }

    public async Task<bool> CancelOrderAsync(Guid orderId, Guid assetId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        // Remove from in-memory book
        var removed = _matchingEngine.CancelOrder(orderId, assetId);

        // Update status in DB
        order.Status = Domain.Enums.OrderStatus.Cancelled;
        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} cancelled", orderId);
        return true;
    }

    public Task<OrderBookResponse> GetOrderBookAsync(Guid assetId)
    {
        var book = _matchingEngine.GetOrderBook(assetId);

        if (book == null)
        {
            return Task.FromResult(new OrderBookResponse(
                AssetId: assetId,
                BestBid: null,
                BestAsk: null,
                Spread: null,
                Bids: Array.Empty<OrderBookLevel>(),
                Asks: Array.Empty<OrderBookLevel>()
            ));
        }

        var bestBid = book.BestBid;
        var bestAsk = book.BestAsk;
        var spread = bestBid.HasValue && bestAsk.HasValue ? bestAsk.Value - bestBid.Value : (decimal?)null;

        var bids = book.GetBidLevels()
            .Select(l => new OrderBookLevel(l.Price, l.TotalQuantity))
            .ToList();

        var asks = book.GetAskLevels()
            .Select(l => new OrderBookLevel(l.Price, l.TotalQuantity))
            .ToList();

        return Task.FromResult(new OrderBookResponse(
            AssetId: assetId,
            BestBid: bestBid,
            BestAsk: bestAsk,
            Spread: spread,
            Bids: bids,
            Asks: asks
        ));
    }
}
