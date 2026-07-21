using System.Text;
using LogiTrack.Data;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<LogiTrackContext>(options => options.UseSqlite(
    builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=logitrack.db"));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<LogiTrackContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
        {
            throw new InvalidOperationException("Jwt:Key must contain at least 32 bytes.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "LogiTrack",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "LogiTrack.Client",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (!app.Environment.IsEnvironment("Testing"))
{
    await InitializeDatabaseAsync(app.Services, builder.Configuration, app.Environment.IsDevelopment());
}

app.Run();

static async Task InitializeDatabaseAsync(
    IServiceProvider services,
    IConfiguration configuration,
    bool seedSampleInventory)
{
    await using var scope = services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<LogiTrackContext>();
    await context.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "User", "Manager" })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Could not create role {role}.");
            }
        }
    }

    var managerEmail = configuration["SeedManager:Email"];
    var managerPassword = configuration["SeedManager:Password"];
    if (!string.IsNullOrWhiteSpace(managerEmail) && !string.IsNullOrWhiteSpace(managerPassword))
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var manager = await userManager.FindByEmailAsync(managerEmail);
        if (manager is null)
        {
            manager = new ApplicationUser { UserName = managerEmail, Email = managerEmail };
            var createResult = await userManager.CreateAsync(manager, managerPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException("Could not create the configured manager account.");
            }
        }

        if (!await userManager.IsInRoleAsync(manager, "Manager"))
        {
            var roleResult = await userManager.AddToRoleAsync(manager, "Manager");
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException("Could not assign the Manager role.");
            }
        }
    }

    if (seedSampleInventory && !await context.InventoryItems.AnyAsync())
    {
        var sample = new InventoryItem
        {
            Name = "Pallet Jack",
            Quantity = 12,
            Location = "Warehouse A"
        };
        context.InventoryItems.Add(sample);
        await context.SaveChangesAsync();
        sample.DisplayInfo();
    }
}

public partial class Program;
