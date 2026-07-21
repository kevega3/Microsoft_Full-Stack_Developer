# 1. Project Title

**LogiTrack Order Management API**

# 2. Describe Your API Project and Its Key Features

LogiTrack is an ASP.NET Core Web API for managing inventory and customer orders across logistics distribution centers. It provides persistent data storage, secure access, role-based permissions, and optimized read operations.

Its key features are:

- **Inventory management:** Authenticated users can view inventory. Managers can create and delete inventory items. Each item includes a name, available quantity, and warehouse location.
- **Order management:** Authenticated users can create and view orders containing multiple inventory items. Managers can delete orders. The API prevents an inventory item from being deleted while it is associated with an order.
- **Authentication and authorization:** Users can register and log in to receive a JWT. The API distinguishes between the `User` and `Manager` roles and restricts sensitive operations to Managers.
- **Validation and error handling:** The API validates request data and returns appropriate HTTP responses such as `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, and `409 Conflict`.
- **Caching and persistence:** Inventory reads are cached in memory, while SQLite remains the persistent source of truth across application restarts.

The project uses .NET 10, ASP.NET Core controllers, Entity Framework Core, SQLite, ASP.NET Core Identity, JWT Bearer authentication, `IMemoryCache`, OpenAPI, and xUnit.

# 3. What Were the Major Challenges You Faced, and How Did You Overcome Them?

One major challenge was modeling the relationship between orders and inventory. An inventory item can appear in multiple orders, and an order can contain multiple inventory items. I implemented this as an EF Core many-to-many relationship. I also added a business check that returns `409 Conflict` when a Manager attempts to delete an inventory item that is still used by an order. This preserves order history and avoids invalid references.

Another challenge was combining ASP.NET Core Identity with JWT authentication. Identity manages users, password hashing, password rules, lockouts, and roles, while JWT is used to authenticate API requests. I configured JWT Bearer as the default authentication scheme and included each user's roles as claims in the token. This allows `[Authorize]` and `[Authorize(Roles = "Manager")]` to enforce access rules consistently.

A third challenge was preventing stale inventory data after introducing caching. The inventory list is cached for 30 seconds, but a successful create or delete operation must be visible immediately. I solved this by invalidating the inventory cache key only after `SaveChangesAsync` succeeds. The next GET request then reloads fresh data from SQLite.

I also had to ensure that two new inventory objects without database IDs were not treated as duplicates. The `Order.AddItem` logic now compares object identity for new items and database IDs for persisted items. Automated tests verify both cases.

# 4. How Did You Implement Key Components Like Business Logic, Data Persistence, and State Management?

The main business entities are `InventoryItem` and `Order`. `InventoryItem` stores the item name, quantity, and location, and its `DisplayInfo()` method prints a formatted summary. `Order` stores the customer name, placement date, and inventory collection. It provides `AddItem`, `RemoveItem`, and `GetOrderSummary` methods.

Controllers use request and response DTOs instead of exposing EF Core entities directly. This prevents overposting and ensures that server-controlled fields, including IDs and `DatePlaced`, cannot be assigned by clients. When an order is created, the API removes duplicate IDs, loads all requested inventory items in one database query, rejects missing items, and records the placement time in UTC.

Data persistence is implemented through `LogiTrackContext`, which inherits from `IdentityDbContext<ApplicationUser>`. It stores inventory, orders, users, roles, and the many-to-many order-item relationship in SQLite. EF Core migrations create and update the database schema. Database constraints enforce required fields, maximum lengths, and non-negative inventory quantities.

SQLite provides persistent state across requests and application restarts. `IMemoryCache` is treated only as a temporary optimization. If the application restarts and the cache is empty, the next inventory request reloads the data from SQLite. Tests use real temporary SQLite files and verify that data remains available through separate context instances.

# 5. What Security Measures Did You Implement?

The API uses ASP.NET Core Identity to store password hashes rather than plain-text passwords. Identity enforces unique email addresses, a minimum password length, uppercase and lowercase characters, a number, a non-alphanumeric character, and a five-minute lockout after five failed login attempts.

Successful login creates a signed JWT using HMAC SHA-256. Token validation checks the signing key, issuer, audience, lifetime, and expiration. The signing key is not stored in source control; it must be supplied through .NET User Secrets or environment variables and must contain at least 32 bytes.

Authorization is role-based. New registrations always receive the `User` role and cannot select their own role. The `Manager` account is created only from protected configuration. Inventory creation and deletion and order deletion require the `Manager` role, while read operations and order creation require authentication.

Input validation is applied at the API boundary using data annotations and explicit business checks. The API rejects invalid email addresses, missing or blank text, negative quantities, oversized values, empty orders, duplicate inventory IDs, and references to nonexistent inventory. DTOs prevent mass assignment, parameterized EF Core queries protect against SQL injection, and HTTPS redirection is enabled.

The security behavior is covered by integration tests for unauthenticated access, valid registration and login, regular-user restrictions, Manager permissions, and invalid request data. NuGet dependency auditing also reports no known vulnerable packages from the configured sources.

# 6. How Did You Manage Caching and Optimize Performance?

The `GET /api/inventory` endpoint uses `IMemoryCache` because the inventory list is read frequently and changes less often than it is requested. The endpoint caches response DTOs for 30 seconds instead of caching tracked EF Core entities. Successful inventory create and delete operations remove the cache entry so that the next request receives current data.

Read-only EF Core queries use `AsNoTracking()` to avoid unnecessary change-tracking overhead. Order queries use `Include(order => order.Items)` so related item details are loaded eagerly and the API avoids N+1 database queries. Order creation uses one `Contains` query to retrieve all requested inventory items instead of querying each item individually.

Database operations are asynchronous and accept `CancellationToken`, allowing abandoned HTTP requests to stop database work. Results are ordered consistently, and DTO projections limit the inventory query to the fields returned by the API.

Caching behavior is verified by an integration test that first warms the cache, changes SQLite directly, confirms that the cached response is reused, then creates an item through the API and confirms that cache invalidation reloads the latest database state.
