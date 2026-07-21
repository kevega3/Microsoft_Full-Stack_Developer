using LogiTrack.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Data;

public sealed class LogiTrackContext(DbContextOptions<LogiTrackContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(item => item.ItemId);
            entity.Property(item => item.Name).HasMaxLength(100).IsRequired();
            entity.Property(item => item.Location).HasMaxLength(100).IsRequired();
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_InventoryItems_Quantity_NonNegative",
                "Quantity >= 0"));
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(order => order.OrderId);
            entity.Property(order => order.CustomerName).HasMaxLength(100).IsRequired();
            entity.Property(order => order.DatePlaced).IsRequired();
            entity.HasMany(order => order.Items)
                .WithMany(item => item.Orders)
                .UsingEntity("OrderItems");
        });
    }
}
