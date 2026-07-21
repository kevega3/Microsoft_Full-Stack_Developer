using LogiTrack.Models;
using LogiTrack.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Tests;

public sealed class DomainModelTests
{
    [Fact]
    public void DisplayInfo_PrintsInventoryDetails()
    {
        var item = new InventoryItem
        {
            Name = "Pallet Jack",
            Quantity = 12,
            Location = "Warehouse A"
        };
        var output = new StringWriter();
        var originalOutput = Console.Out;

        try
        {
            Console.SetOut(output);
            item.DisplayInfo();
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        Assert.Equal($"Item: Pallet Jack | Quantity: 12 | Location: Warehouse A{Environment.NewLine}", output.ToString());
    }

    [Fact]
    public void AddItem_DoesNotAddTheSameInventoryItemTwice()
    {
        var order = new Order();
        var item = new InventoryItem { ItemId = 10 };

        order.AddItem(item);
        order.AddItem(item);

        Assert.Single(order.Items);
    }

    [Fact]
    public void AddItem_AddsDistinctItemsThatDoNotHaveDatabaseIdsYet()
    {
        var order = new Order();

        order.AddItem(new InventoryItem { Name = "Pallet Jack" });
        order.AddItem(new InventoryItem { Name = "Hand Truck" });

        Assert.Equal(2, order.Items.Count);
    }

    [Fact]
    public void RemoveItem_RemovesAnItemById()
    {
        var order = new Order();
        order.AddItem(new InventoryItem { ItemId = 10 });

        order.RemoveItem(10);

        Assert.Empty(order.Items);
    }

    [Fact]
    public void GetOrderSummary_ReturnsTheRequiredFormat()
    {
        var order = new Order
        {
            OrderId = 1001,
            CustomerName = "Samir",
            DatePlaced = new DateTime(2024, 5, 4, 0, 0, 0, DateTimeKind.Utc)
        };
        order.AddItem(new InventoryItem { ItemId = 10 });
        order.AddItem(new InventoryItem { ItemId = 20 });

        var summary = order.GetOrderSummary();

        Assert.Equal("Order #1001 for Samir | Items: 2 | Placed: 5/4/2024", summary);
    }

    [Fact]
    public async Task LogiTrackContext_PersistsAnOrderWithInventoryItems()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"logitrack-domain-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<LogiTrackContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        try
        {
            await using (var writeContext = new LogiTrackContext(options))
            {
                await writeContext.Database.EnsureCreatedAsync();
                var item = new InventoryItem
                {
                    Name = "Pallet Jack",
                    Quantity = 12,
                    Location = "Warehouse A"
                };
                writeContext.Orders.Add(new Order
                {
                    CustomerName = "Samir",
                    DatePlaced = DateTime.UtcNow,
                    Items = [item]
                });
                await writeContext.SaveChangesAsync();
            }

            await using var readContext = new LogiTrackContext(options);
            var stored = await readContext.Orders.Include(value => value.Items).SingleAsync();

            Assert.Equal("Samir", stored.CustomerName);
            Assert.Equal("Pallet Jack", Assert.Single(stored.Items).Name);
        }
        finally
        {
            File.Delete(databasePath);
        }
    }
}
