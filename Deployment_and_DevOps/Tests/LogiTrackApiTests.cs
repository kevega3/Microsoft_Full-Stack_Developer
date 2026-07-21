using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LogiTrack.Data;
using LogiTrack.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogiTrack.Tests;

public sealed class LogiTrackApiTests
{
    private const string Password = "Valid1!Password";

    [Fact]
    public async Task Inventory_WithoutToken_ReturnsUnauthorized()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();

        var response = await client.GetAsync("/api/inventory");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAndLogin_WithValidCredentials_ReturnsJwt()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();

        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "user@example.com",
            Password
        });
        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "user@example.com",
            Password
        });

        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(await ReadStringPropertyAsync(login, "accessToken")));
    }

    [Fact]
    public async Task InventoryPost_WithRegularUser_ReturnsForbidden()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await RegisterAsync(client, "user@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await LoginAsync(client, "user@example.com"));

        var response = await client.PostAsJsonAsync("/api/inventory", new
        {
            Name = "Pallet Jack",
            Quantity = 12,
            Location = "Warehouse A"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Manager_CanManageInventoryAndOrders()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.CreateManagerAsync("manager@example.com", Password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await LoginAsync(client, "manager@example.com"));

        var inventoryResponse = await client.PostAsJsonAsync("/api/inventory", new
        {
            Name = "Pallet Jack",
            Quantity = 12,
            Location = "Warehouse A"
        });
        Assert.Equal(HttpStatusCode.Created, inventoryResponse.StatusCode);
        var itemId = await ReadIntPropertyAsync(inventoryResponse, "itemId");
        var secondInventoryResponse = await client.PostAsJsonAsync("/api/inventory", new
        {
            Name = "Hand Truck",
            Quantity = 5,
            Location = "Warehouse B"
        });
        Assert.Equal(HttpStatusCode.Created, secondInventoryResponse.StatusCode);
        var secondItemId = await ReadIntPropertyAsync(secondInventoryResponse, "itemId");

        var orderResponse = await client.PostAsJsonAsync("/api/orders", new
        {
            CustomerName = "Samir",
            InventoryItemIds = new[] { itemId, secondItemId }
        });
        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);
        var orderId = await ReadIntPropertyAsync(orderResponse, "orderId");

        var storedOrder = await client.GetAsync($"/api/orders/{orderId}");
        Assert.Equal(HttpStatusCode.OK, storedOrder.StatusCode);
        var orderDocument = await JsonDocument.ParseAsync(await storedOrder.Content.ReadAsStreamAsync());
        Assert.Equal(2, orderDocument.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal("Pallet Jack", orderDocument.RootElement.GetProperty("items")[0].GetProperty("name").GetString());

        var inventoryInUse = await client.DeleteAsync($"/api/inventory/{itemId}");
        Assert.Equal(HttpStatusCode.Conflict, inventoryInUse.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/orders/{orderId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/inventory/{itemId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/inventory/{secondItemId}")).StatusCode);
    }

    [Fact]
    public async Task InventoryPost_WithNegativeQuantity_ReturnsBadRequest()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.CreateManagerAsync("manager@example.com", Password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await LoginAsync(client, "manager@example.com"));

        var response = await client.PostAsJsonAsync("/api/inventory", new
        {
            Name = "Pallet Jack",
            Quantity = -1,
            Location = "Warehouse A"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InventoryCache_IsReusedAndInvalidatedAfterCreate()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();
        await factory.CreateManagerAsync("manager@example.com", Password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await LoginAsync(client, "manager@example.com"));

        Assert.Empty(await ReadInventoryNamesAsync(await client.GetAsync("/api/inventory")));
        await factory.AddInventoryDirectlyAsync("Hand Truck");

        Assert.Empty(await ReadInventoryNamesAsync(await client.GetAsync("/api/inventory")));

        var creation = await client.PostAsJsonAsync("/api/inventory", new
        {
            Name = "Forklift",
            Quantity = 2,
            Location = "Warehouse B"
        });
        creation.EnsureSuccessStatusCode();

        Assert.Equal(
            new[] { "Hand Truck", "Forklift" },
            await ReadInventoryNamesAsync(await client.GetAsync("/api/inventory")));
    }

    private static async Task RegisterAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new { Email = email, Password });
        response.EnsureSuccessStatusCode();
    }

    private static async Task<string> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password });
        response.EnsureSuccessStatusCode();
        return await ReadStringPropertyAsync(response, "accessToken");
    }

    private static async Task<string> ReadStringPropertyAsync(HttpResponseMessage response, string propertyName)
    {
        var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty(propertyName).GetString()!;
    }

    private static async Task<int> ReadIntPropertyAsync(HttpResponseMessage response, string propertyName)
    {
        var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty(propertyName).GetInt32();
    }

    private static async Task<string[]> ReadInventoryNamesAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement
            .EnumerateArray()
            .Select(item => item.GetProperty("name").GetString()!)
            .ToArray();
    }
}

internal sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"logitrack-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}",
                ["Jwt:Key"] = "A-development-only-test-key-with-at-least-32-bytes!",
                ["Jwt:Issuer"] = "LogiTrack.Tests",
                ["Jwt:Audience"] = "LogiTrack.Tests"
            }));
    }

    public async Task InitializeDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<LogiTrackContext>();
        await context.Database.EnsureCreatedAsync();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "User", "Manager" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                Assert.True((await roleManager.CreateAsync(new IdentityRole(role))).Succeeded);
            }
        }
    }

    public async Task CreateManagerAsync(string email, string password)
    {
        await using var scope = Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = email, Email = email };
        Assert.True((await userManager.CreateAsync(user, password)).Succeeded);
        Assert.True((await userManager.AddToRoleAsync(user, "Manager")).Succeeded);
    }

    public async Task AddInventoryDirectlyAsync(string name)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<LogiTrackContext>();
        context.InventoryItems.Add(new InventoryItem
        {
            Name = name,
            Quantity = 1,
            Location = "Warehouse A"
        });
        await context.SaveChangesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
