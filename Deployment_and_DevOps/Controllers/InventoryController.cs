using System.ComponentModel.DataAnnotations;
using LogiTrack.Data;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LogiTrack.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory")]
public sealed class InventoryController(LogiTrackContext context, IMemoryCache cache) : ControllerBase
{
    private const string InventoryCacheKey = "inventory:list";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InventoryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(InventoryCacheKey, out IReadOnlyList<InventoryResponse>? inventory))
        {
            inventory = await context.InventoryItems
                .AsNoTracking()
                .OrderBy(item => item.ItemId)
                .Select(item => new InventoryResponse(item.ItemId, item.Name, item.Quantity, item.Location))
                .ToListAsync(cancellationToken);
            cache.Set(InventoryCacheKey, inventory, TimeSpan.FromSeconds(30));
        }

        return Ok(inventory);
    }

    [HttpPost]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<InventoryResponse>> Create(
        CreateInventoryRequest request,
        CancellationToken cancellationToken)
    {
        if (!ValidateText(request.Name, nameof(request.Name)) ||
            !ValidateText(request.Location, nameof(request.Location)))
        {
            return ValidationProblem(ModelState);
        }

        var item = new InventoryItem
        {
            Name = request.Name.Trim(),
            Quantity = request.Quantity,
            Location = request.Location.Trim()
        };
        context.InventoryItems.Add(item);
        await context.SaveChangesAsync(cancellationToken);
        cache.Remove(InventoryCacheKey);

        return Created(
            "/api/inventory",
            new InventoryResponse(item.ItemId, item.Name, item.Quantity, item.Location));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var item = await context.InventoryItems.FindAsync([id], cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        if (await context.Orders.AnyAsync(
                order => order.Items.Any(inventoryItem => inventoryItem.ItemId == id),
                cancellationToken))
        {
            return Conflict(new ProblemDetails { Title = "Inventory item is used by an order." });
        }

        context.InventoryItems.Remove(item);
        await context.SaveChangesAsync(cancellationToken);
        cache.Remove(InventoryCacheKey);
        return NoContent();
    }

    private bool ValidateText(string value, string propertyName)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        ModelState.AddModelError(propertyName, $"{propertyName} cannot be blank.");
        return false;
    }
}

public sealed class CreateInventoryRequest
{
    [Required, StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Quantity { get; init; }

    [Required, StringLength(100)]
    public string Location { get; init; } = string.Empty;
}

public sealed record InventoryResponse(int ItemId, string Name, int Quantity, string Location);
