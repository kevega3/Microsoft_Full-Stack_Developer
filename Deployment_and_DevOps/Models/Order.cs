using System.Globalization;

namespace LogiTrack.Models;

public sealed class Order
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime DatePlaced { get; set; }
    public List<InventoryItem> Items { get; set; } = [];

    public void AddItem(InventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!Items.Contains(item) &&
            (item.ItemId == 0 || Items.All(existing => existing.ItemId != item.ItemId)))
        {
            Items.Add(item);
        }
    }

    public void RemoveItem(int itemId)
    {
        Items.RemoveAll(item => item.ItemId == itemId);
    }

    public string GetOrderSummary()
    {
        return $"Order #{OrderId} for {CustomerName} | Items: {Items.Count} | Placed: {DatePlaced.ToString("M/d/yyyy", CultureInfo.InvariantCulture)}";
    }
}
