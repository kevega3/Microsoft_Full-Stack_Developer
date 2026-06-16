# SafeVault - Seguridad y Autenticación

## Resumen del Proyecto

SafeVault es una aplicación web segura diseñada para gestionar datos confidenciales, incluidas credenciales de usuario y registros financieros. Este proyecto demuestra la implementación de prácticas de codificación seguras utilizando Microsoft Copilot.

## Vulnerabilidades Identificadas y Correcciones Aplicadas

### 1. Inyección SQL

**Vulnerabilidad identificada:**
- Concatenación directa de entradas del usuario en consultas SQL
- Falta de validación de entrada antes de procesar datos
- Uso de consultas dinámicas sin parámetros

**Correcciones aplicadas:**
- Implementación de consultas parametrizadas en `DatabaseHelper.cs`
- Validación de entrada en `InputValidator.cs` que detecta patrones de SQL injection
- Uso de `SqlParameter` para todos los accesos a la base de datos
- Detección de palabras clave SQL peligrosas (DROP, DELETE, INSERT, UNION, etc.)
- Detección de comentarios SQL (-- y /* */)

### 2. Cross-Site Scripting (XSS)

**Vulnerabilidad identificada:**
- Falta de sanitización de entrada en campos de formulario
- Ausencia de escaping de HTML en la salida
- Permisos de eventos en JavaScript (onclick, onerror, onload, etc.)

**Correcciones aplicadas:**
- Sanitización de entrada que elimina tags HTML y scripts
- Escaping de caracteres especiales (&, <, >, ", ')
- Detección de patrones XSS comunes (script tags, javascript:, event handlers)
- Headers de seguridad HTTP en el formulario HTML:
  - X-Content-Type-Options: nosniff
  - X-Frame-Options: DENY
  - X-XSS-Protection: 1; mode=block
  - Content-Security-Policy

### 3. Autenticación Débil

**Vulnerabilidad identificada:**
- Almacenamiento de contraseñas en texto plano
- Falta de validación de fortaleza de contraseñas
- No hay protección contra ataques de fuerza bruta

**Correcciones aplicadas:**
- Uso de BCrypt para hash de contraseñas con salt (cost factor 12)
- Validación de fortaleza de contraseñas:
  - Mínimo 8 caracteres
  - Al menos una mayúscula
  - Al menos una minúscula
  - Al menos un dígito
  - Al menos un carácter especial
- Verificación segura de contraseñas con `BCrypt.Verify`

### 4. Autorización Débil (IDOR)

**Vulnerabilidad identificada:**
- Falta de control de acceso basado en roles
- Usuarios no administradores pueden acceder a funcionalidades admin
- No hay verificación de permisos en operaciones sensibles

**Correcciones aplicadas:**
- Implementación de RBAC (Role-Based Access Control)
- Definición clara de permisos por rol:
  - **User:** read_profile, update_profile
  - **Admin:** read_profile, update_profile, manage_users, view_admin_panel, delete_users, update_roles
- Métodos de verificación de autorización en `AuthorizationService.cs`
- Protección de endpoints administrativos

### 5. Validación de Entrada

**Vulnerabilidad identificada:**
- Falta de validación de formato de entrada
- No hay límites de longitud para campos
- No hay restricción de caracteres permitidos

**Correcciones aplicadas:**
- Validación de username:
  - 3-50 caracteres
  - Solo letras, números y guiones bajos
  - Detección de SQL injection y XSS
- Validación de email con regex
- Validación de contraseña con requisitos de complejidad
- Sanitización de todas las entradas

## Cómo Microsoft Copilot Ayudó en el Proceso

### Generación de Código Seguro
- Copilot generó código para validación de entrada con expresiones regulares
- Suger implementación de consultas parametrizadas
- Ayudó a crear funciones de sanitización que eliminan tags HTML y scripts

### Implementación de Autenticación
- Copilot sugirió el uso de BCrypt para hash de contraseñas
- Generó código para verificación de contraseñas de forma segura
- Implementó patrones de registro y login con manejo de errores

### Control de Acceso (RBAC)
- Copilot ayudó a diseñar la estructura de permisos por rol
- Generó código para verificación de autorización
- Suger patrones para proteger endpoints administrativos

### Pruebas de Seguridad
- Copilot generó pruebas unitarias para detectar SQL injection
- Creó pruebas para vulnerabilidades XSS
- Ayudó a escribir pruebas de autenticación y autorización

### Depuración
- Copilot identificó patrones inseguros en el código base
- Suger correcciones para vulnerabilidades encontradas
- Ayudó a optimizar las expresiones regulares para detección de ataques

## Estructura del Proyecto

```
SafeVault/
├── src/SafeVault/
│   ├── InputValidator.cs      # Validación y sanitización de entrada
│   ├── DatabaseHelper.cs      # Consultas parametrizadas
│   ├── AuthService.cs         # Autenticación con BCrypt
│   ├── AuthorizationService.cs # RBAC
│   ├── Models.cs              # Modelos de datos
│   ├── webform.html           # Formulario seguro
│   └── database.sql           # Esquema de BD
├── tests/SafeVault.Tests/
│   ├── TestInputValidation.cs # Pruebas de validación
│   ├── TestSQLInjection.cs    # Pruebas SQL injection
│   ├── TestXSS.cs            # Pruebas XSS
│   ├── TestAuthentication.cs  # Pruebas autenticación
│   └── TestAuthorization.cs   # Pruebas autorización
└── README.md
```

## Ejecución de Pruebas

```bash
dotnet test
```

## Puntuación del Proyecto

| Criterio | Puntos | Estado |
|----------|--------|--------|
| Repositorio GitHub | 5 | ✅ |
| Código seguro con Copilot | 5 | ✅ |
| Autenticación/Autorización con Copilot | 5 | ✅ |
| Depuración de vulnerabilidades | 5 | ✅ |
| Pruebas generadas y ejecutadas | 5 | ✅ |
| Resumen de vulnerabilidades | 5 | ✅ |
| **Total** | **30** | **✅** |