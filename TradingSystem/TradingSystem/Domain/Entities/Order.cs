using TradingSystem.Domain.Enums;

namespace TradingSystem.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public Guid UserId { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public OrderSide Side { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Asset? Asset { get; set; }
    public ICollection<Trade> BuyTrades { get; set; } = new List<Trade>();
    public ICollection<Trade> SellTrades { get; set; } = new List<Trade>();

    public void Fill(decimal quantity)
    {
        RemainingQuantity -= quantity;

        Status = RemainingQuantity <= 0
            ? OrderStatus.Filled
            : OrderStatus.PartiallyFilled;
    }
}
