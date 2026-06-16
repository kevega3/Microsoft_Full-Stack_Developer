var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();

app.MapGet("/api/productlist", (IMemoryCache cache) =>
{
    const string cacheKey = "productos";
    
    if (cache.TryGetValue(cacheKey, out List<object> productosCache))
    {
        return Results.Ok(productosCache);
    }

    var productos = new[]
    {
        new
        {
            Id = 1,
            Nombre = "Portátil",
            Precio = 1200.50,
            Stock = 25,
            Categoria = new { Id = 101, Nombre = "Electrónica" }
        },
        new
        {
            Id = 2,
            Nombre = "Auriculares",
            Precio = 50.00,
            Stock = 100,
            Categoria = new { Id = 102, Nombre = "Accesorios" }
        }
    };

    var opcionesCache = new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        SlidingExpiration = TimeSpan.FromMinutes(2)
    };

    cache.Set(cacheKey, productos.ToList(), opcionesCache);

    return Results.Ok(productos);
})
.WithName("GetProductos")
.Produces<List<object>>(StatusCodes.Status200OK);

app.Run();