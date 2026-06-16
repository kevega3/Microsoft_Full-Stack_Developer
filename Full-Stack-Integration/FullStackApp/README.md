# InventoryHub - Sistema de Gestión de Inventario

Aplicación full-stack de gestión de inventario construida con **Blazor WebAssembly** (front-end) y **Minimal API .NET 10** (back-end).

## Tecnologías

- **.NET 10** (ASP.NET Core)
- **Blazor WebAssembly** - Front-end SPA
- **Minimal API** - Back-end REST
- **MemoryCache** - Caché en memoria del servidor
- **System.Text.Json** - Serialización JSON

## Estructura del proyecto

```
FullStackApp/
├── FullStackSolution.sln
├── ClientApp/                 # Blazor WebAssembly (Front-end)
│   ├── Pages/
│   │   └── FetchProducts.razor  # Componente de lista de productos
│   ├── Layout/
│   │   └── NavMenu.razor        # Navegación principal
│   ├── Program.cs               # Configuración HttpClient
│   └── wwwroot/
└── ServerApp/                   # Minimal API (Back-end)
    ├── Program.cs               # Endpoints, CORS, Caché
    └── appsettings.json
```

## Requisitos previos

- .NET 10 SDK (10.0.100 o superior)

## Ejecución

### 1. Clonar y restaurar dependencias

```bash
cd FullStackApp
dotnet restore
```

### 2. Ejecutar el back-end (ServerApp)

```bash
cd ServerApp
dotnet run
```

El back-end se ejecutará en `https://localhost:7000` (puertos pueden variar).

Verificar endpoint: `https://localhost:7000/api/productlist`

### 3. Ejecutar el front-end (ClientApp)

En una terminal separada:

```bash
cd ClientApp
dotnet run
```

El front-end se ejecutará en `https://localhost:5001` (puertos pueden variar).

### 4. Acceder a la aplicación

Abrir el navegador en la URL del front-end (ej. `https://localhost:5001`) y navegar a **Productos** en el menú lateral.

## API Endpoints

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/productlist` | Obtiene lista de productos con categorías |

### Ejemplo de respuesta JSON

```json
[
  {
    "id": 1,
    "nombre": "Portátil",
    "precio": 1200.50,
    "stock": 25,
    "categoria": {
      "id": 101,
      "nombre": "Electrónica"
    }
  },
  {
    "id": 2,
    "nombre": "Auriculares",
    "precio": 50.00,
    "stock": 100,
    "categoria": {
      "id": 102,
      "nombre": "Accesorios"
    }
  }
]
```

## Características implementadas

- ✅ Comunicación front-end/back-end vía HTTP/JSON
- ✅ Política CORS configurada (AllowAnyOrigin)
- ✅ Manejo de errores robusto (try-catch, HttpRequestException, JsonException)
- ✅ Caché en memoria del servidor (MemoryCache, 10 min expiración absoluta, 2 min deslizante)
- ✅ Estructura JSON con objeto anidado (Categoria)
- ✅ Deserialización case-insensitive
- ✅ Navegación integrada en menú lateral

## Desarrollo asistido por Copilot

Este proyecto fue desarrollado con asistencia de **Microsoft Copilot** en las siguientes áreas:

1. **Generación de código de integración** - HttpClient, deserialización JSON
2. **Depuración** - Corrección de rutas API, configuración CORS, manejo JSON malformado
3. **Estructuración JSON** - Objetos anidados, validación de formato
4. **Optimización** - Caché MemoryCache, reducción de llamadas redundantes
5. **Documentación** - Comentarios explicativos, REFLECTION.md

## Licencia

Proyecto educativo - Taller Full-Stack Integration