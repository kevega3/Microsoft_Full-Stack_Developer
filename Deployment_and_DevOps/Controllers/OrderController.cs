using System.ComponentModel.DataAnnotations;
using LogiTrack.Data;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrderController(LogiTrackContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var orders = await context.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .OrderBy(order => order.OrderId)
            .ToListAsync(cancellationToken);

        return Ok(orders.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .AsNoTracking()
            .Include(value => value.Items)
            .SingleOrDefaultAsync(value => value.OrderId == id, cancellationToken);

        return order is null ? NotFound() : Ok(ToResponse(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            ModelState.AddModelError(nameof(request.CustomerName), "CustomerName cannot be blank.");
            return ValidationProblem(ModelState);
        }

        var itemIds = request.InventoryItemIds.Distinct().ToArray();
        if (itemIds.Length != request.InventoryItemIds.Length)
        {
            ModelState.AddModelError(nameof(request.InventoryItemIds), "Inventory item IDs must be unique.");
            return ValidationProblem(ModelState);
        }

        var items = await context.InventoryItems
            .Where(item => itemIds.Contains(item.ItemId))
            .ToListAsync(cancellationToken);
        if (items.Count != itemIds.Length)
        {
            ModelState.AddModelError(nameof(request.InventoryItemIds), "One or more inventory items do not exist.");
            return ValidationProblem(ModelState);
        }

        var order = new Order
        {
            CustomerName = request.CustomerName.Trim(),
            DatePlaced = DateTime.UtcNow,
            Items = items
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, ToResponse(order));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var order = await context.Orders.FindAsync([id], cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        context.Orders.Remove(order);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static OrderResponse ToResponse(Order order)
    {
        return new OrderResponse(
            order.OrderId,
            order.CustomerName,
            order.DatePlaced,
            order.Items
                .OrderBy(item => item.ItemId)
                .Select(item => new InventoryResponse(item.ItemId, item.Name, item.Quantity, item.Location))
                .ToArray());
    }
}

public sealed class CreateOrderRequest
{
    [Required, StringLength(100)]
    public string CustomerName { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public int[] InventoryItemIds { get; init; } = [];
}

public sealed record OrderResponse(
    int OrderId,
    string CustomerName,
    DateTime DatePlaced,
    IReadOnlyList<InventoryResponse> Items);
