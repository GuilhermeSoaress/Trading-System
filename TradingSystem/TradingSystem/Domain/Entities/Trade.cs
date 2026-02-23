namespace TradingSystem.Domain.Entities;

public class Trade
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public Guid BuyOrderId { get; set; }
    public Guid SellOrderId { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    public Asset? Asset { get; set; }
    public Order? BuyOrder { get; set; }
    public Order? SellOrder { get; set; }
}
