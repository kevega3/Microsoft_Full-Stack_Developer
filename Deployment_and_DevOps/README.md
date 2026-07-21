# LogiTrack Order Management API

LogiTrack es una API ASP.NET Core para administrar inventario y pedidos de una plataforma logistica con varios centros de distribucion. El proyecto se encuentra directamente en `Deployment_and_DevOps/`; no utiliza una carpeta contenedora adicional.

## Funcionalidades principales

1. Gestion de inventario mediante consulta, creacion y eliminacion de articulos.
2. Gestion de pedidos que pueden incluir varios articulos de inventario.
3. Registro, inicio de sesion JWT y autorizacion por roles `User` y `Manager`.

La lista de inventario usa cache en memoria y todos los datos importantes permanecen en SQLite despues de reiniciar la aplicacion.

## Tecnologias

- .NET 10 y ASP.NET Core Web API con controladores.
- Entity Framework Core 10 y SQLite.
- ASP.NET Core Identity y JWT Bearer.
- `IMemoryCache`.
- OpenAPI integrado.
- xUnit y `WebApplicationFactory`.

## Configuracion

La clave JWT y las credenciales del Manager no se guardan en el repositorio. Configuralas con User Secrets:

```bash
dotnet user-secrets set "Jwt:Key" "replace-with-a-random-secret-of-at-least-32-bytes"
dotnet user-secrets set "SeedManager:Email" "manager@example.com"
dotnet user-secrets set "SeedManager:Password" "replace-with-a-strong-password"
```

Restaura herramientas y dependencias, aplica la migracion y ejecuta la API:

```bash
dotnet tool restore
dotnet restore
dotnet tool run dotnet-ef database update
dotnet run --project LogiTrack.csproj
```

En desarrollo se crea el articulo de muestra `Pallet Jack` si el inventario esta vacio. La especificacion OpenAPI queda disponible en `http://localhost:5193/openapi/v1.json` y puede importarse en Postman. `LogiTrack.http` contiene un flujo completo reproducible.

## API

| Metodo | Ruta | Acceso | Funcion |
|---|---|---|---|
| POST | `/api/auth/register` | Anonimo | Registra un usuario con rol `User` |
| POST | `/api/auth/login` | Anonimo | Devuelve un JWT |
| GET | `/api/inventory` | Autenticado | Lista el inventario |
| POST | `/api/inventory` | `Manager` | Crea un articulo |
| DELETE | `/api/inventory/{id}` | `Manager` | Elimina un articulo no utilizado |
| GET | `/api/orders` | Autenticado | Lista pedidos con sus articulos |
| GET | `/api/orders/{id}` | Autenticado | Obtiene un pedido con detalles |
| POST | `/api/orders` | Autenticado | Crea un pedido |
| DELETE | `/api/orders/{id}` | `Manager` | Elimina un pedido |

La API devuelve `400` para entradas invalidas, `401` sin autenticacion, `403` sin el rol necesario, `404` para IDs inexistentes y `409` al intentar borrar inventario asociado a un pedido.

## Arquitectura y logica de negocio

`InventoryItem` contiene nombre, cantidad y ubicacion. Su metodo `DisplayInfo()` imprime el formato solicitado por la actividad. `Order` contiene cliente, fecha UTC y articulos, ademas de `AddItem`, `RemoveItem` y `GetOrderSummary`.

`Order` e `InventoryItem` tienen una relacion muchos-a-muchos administrada por EF Core. Esta relacion permite reutilizar un articulo de inventario en varios pedidos sin crear copias. No se creo una entidad `OrderItem` porque el alcance no solicita cantidad ni precio por linea.

Los controladores reciben y devuelven DTO en lugar de entidades de EF. Esto evita overposting y permite controlar los campos establecidos por el servidor, como IDs y `DatePlaced`.

## Seguridad

Identity almacena hashes de contrasena y aplica requisitos de complejidad, email unico y bloqueo despues de cinco intentos fallidos. El login usa `SignInManager`, genera un JWT firmado con HMAC SHA-256 e incluye los roles del usuario.

El cliente no puede elegir su rol durante el registro. Los nuevos usuarios reciben `User`; el Manager solo se crea desde configuracion segura. Las rutas usan `[Authorize]` y `[Authorize(Roles = "Manager")]`.

Los DTO aplican validacion de email, longitud, campos obligatorios y rangos. Tambien se rechazan cadenas en blanco, pedidos vacios, IDs duplicados e IDs de inventario inexistentes. EF Core aplica longitudes, campos obligatorios y una restriccion SQL que impide cantidades negativas.

## Cache, estado y rendimiento

`GET /api/inventory` almacena DTO sin seguimiento durante 30 segundos. `POST` y `DELETE` invalidan la entrada despues de guardar correctamente. SQLite sigue siendo la fuente de verdad, por lo que la cache se reconstruye despues de reiniciar el proceso.

Las consultas de lectura usan `AsNoTracking()`. Los pedidos usan `Include()` para evitar N+1 y la creacion de pedidos carga todos los IDs mediante una sola consulta `Contains`. Todo el acceso a EF Core es asincrono y acepta `CancellationToken`.

La persistencia se comprueba con SQLite real tanto en desarrollo como en las pruebas. La base `logitrack.db` no se versiona.

## Desafios y soluciones

La primera dificultad fue modelar inventario compartido entre pedidos. Se resolvio con una relacion muchos-a-muchos y una comprobacion que impide borrar articulos utilizados. La segunda fue combinar Identity con JWT: Identity administra usuarios y contrasenas, mientras JWT es el esquema predeterminado para la API. La tercera fue evitar datos obsoletos en cache; las escrituras eliminan la clave solo despues de confirmar `SaveChangesAsync`.

## Pruebas

Ejecuta:

```bash
dotnet test Tests/LogiTrack.Tests.csproj
```

Las pruebas cubren los metodos del dominio, la relacion EF, registro y login, proteccion sin token, permisos por rol, validacion, inventario, pedidos, conflicto de eliminacion y reutilizacion/invalidez de cache. Cada fabrica de API usa su propio archivo SQLite temporal.

## Revision asistida

Antes de la entrega, ejecuta realmente estos prompts en Microsoft Copilot y registra debajo la sugerencia aplicada o rechazada. No se incluye una afirmacion inventada de uso de Copilot.

```text
Review the EF Core many-to-many relationship and identify correctness issues.
Review JWT authentication and role authorization for security weaknesses.
Optimize the inventory and order queries without changing API behavior.
Review this codebase for redundant logic and production-readiness issues.
```

Resultado de la revision de Copilot: pendiente de completar por el autor de la entrega.
