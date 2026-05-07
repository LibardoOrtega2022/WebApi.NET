# WebAPIUser — Documentación del Proyecto

> **Stack:** .NET 10 · ASP.NET Core Web API · Entity Framework Core 10 · SQL Server (LocalDB) · Swagger/OpenAPI

---

## 1. Propósito del Proyecto

WebAPIUser es una API REST construida para gestionar **usuarios** y **tareas**, con soporte para asignar tareas a múltiples usuarios (relación muchos a muchos). El objetivo es demostrar una arquitectura limpia y escalable en .NET 10 con buenas prácticas: separación de responsabilidades, DTOs, servicios, middleware de errores y documentación automática con Swagger.

---

## 2. Arquitectura General

```
┌─────────────────────────────────────────────────────────┐
│                        Cliente                          │
│              (Swagger UI / React / Postman)              │
└────────────────────────┬────────────────────────────────┘
                         │ HTTP
┌────────────────────────▼────────────────────────────────┐
│              ExceptionHandlingMiddleware                 │
│         (captura excepciones no manejadas globalmente)   │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│                    Controllers                           │
│         UsuariosController · TareasController            │
│   (reciben requests, validan, delegan a servicios)       │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│                      Services                            │
│       UsuarioService · TareaService · MapperService      │
│   (lógica de negocio, validaciones, mapeo de datos)      │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│                  DbUserContext (EF Core)                 │
│         ORM que traduce objetos C# a SQL Server          │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│              SQL Server (LocalDB) — db_user              │
│          Tablas: Usuario · Tarea · UsuarioTarea          │
└─────────────────────────────────────────────────────────┘
```

El flujo siempre es: **Controller → Service → DbContext → DB**. Los controllers nunca acceden directamente a la base de datos.

---

## 3. Base de Datos

### Conexión

```
Data Source=(localdb)\MSSQLLocalDB;Database=db_user;Trusted_Connection=True
```

Configurada en `appsettings.json` bajo la clave `connectionDB`. Al iniciar la aplicación, EF Core aplica automáticamente las migraciones pendientes (`db.Database.MigrateAsync()`).

---

### Diagrama de Tablas

```
┌──────────────────────┐        ┌──────────────────────────┐        ┌──────────────────────┐
│       Usuario        │        │      UsuarioTarea         │        │        Tarea         │
├──────────────────────┤        │     (Tabla Pivote)        │        ├──────────────────────┤
│ id          INT PK   │◄──────►│ id              INT PK   │◄──────►│ id          INT PK   │
│ nombres     VARCHAR  │        │ usuario_id      INT FK   │        │ titulo      VARCHAR  │
│ apellidos   VARCHAR  │        │ tarea_id        INT FK   │        │ descripcion VARCHAR  │
│ correo      VARCHAR  │        │ fecha_asignacion DATETIME│        │ prioridad   INT      │
│ username    VARCHAR  │        │ estado          INT      │        │ categoria   VARCHAR  │
│ fecha_creacion DATETIME│       │ fecha_completado DATETIME│        │ fecha_vencimiento    │
└──────────────────────┘        └──────────────────────────┘        │ fecha_creacion       │
                                                                     └──────────────────────┘
```

### Tabla `Usuario`

| Columna | Tipo | Restricciones |
|---|---|---|
| `id` | INT | PK, Identity |
| `nombres` | VARCHAR(50) | NOT NULL |
| `apellidos` | VARCHAR(50) | NOT NULL |
| `correo` | VARCHAR(100) | NOT NULL |
| `username` | VARCHAR(100) | NULL |
| `fecha_creacion` | DATETIME | NULL |

### Tabla `Tarea`

| Columna | Tipo | Restricciones |
|---|---|---|
| `id` | INT | PK, Identity |
| `titulo` | VARCHAR(200) | NOT NULL |
| `descripcion` | VARCHAR(500) | NULL |
| `prioridad` | INT | NOT NULL (enum 1-4) |
| `categoria` | VARCHAR(100) | NULL |
| `fecha_vencimiento` | DATETIME | NULL |
| `fecha_creacion` | DATETIME | NOT NULL |

### Tabla `UsuarioTarea` (Pivote M:M)

| Columna | Tipo | Restricciones |
|---|---|---|
| `id` | INT | PK, Identity |
| `usuario_id` | INT | FK → Usuario, CASCADE DELETE |
| `tarea_id` | INT | FK → Tarea, CASCADE DELETE |
| `fecha_asignacion` | DATETIME | NOT NULL |
| `estado` | INT | NOT NULL (enum 1-4) |
| `fecha_completado` | DATETIME | NULL |

**Cascade Delete:** al eliminar un `Usuario` o una `Tarea`, todos sus registros en `UsuarioTarea` se eliminan automáticamente.

---

### Enums

**`PrioridadEnum`** — prioridad de una tarea:
| Valor | Nombre |
|---|---|
| 1 | Baja |
| 2 | Media |
| 3 | Alta |
| 4 | Critica |

**`EstadoTareaEnum`** — estado de una asignación usuario-tarea:
| Valor | Nombre |
|---|---|
| 1 | Pendiente |
| 2 | EnProgreso |
| 3 | Completada |
| 4 | Cancelada |

---

## 4. Modelos (Entidades)

Ubicados en `Models/`. Representan directamente las tablas de la base de datos.

### `Usuario.cs`
Entidad principal de usuario. Tiene una colección de navegación `UsuarioTareas` para acceder a sus asignaciones.

### `Tarea.cs`
Entidad de tarea. Tiene una colección de navegación `UsuarioTareas` para saber a qué usuarios está asignada.

### `UsuarioTarea.cs`
Tabla pivote que materializa la relación M:M. Además de las FKs, almacena datos propios de la relación: `Estado`, `FechaAsignacion` y `FechaCompletado`.

### `DbUserContext.cs`
El `DbContext` de EF Core. Configura el mapeo entre clases C# y tablas SQL en `OnModelCreating`:
- Nombres de columnas en snake_case (`fecha_creacion`, `usuario_id`, etc.)
- Tipos de columna explícitos (`varchar`, `datetime`)
- Relaciones con cascade delete en `UsuarioTarea`

---

## 5. DTOs (Data Transfer Objects)

Ubicados en `DTOs/`. Separan la representación de la API del modelo interno de la base de datos. Esto evita exponer campos internos y permite validar la entrada de forma independiente al modelo.

| DTO | Uso |
|---|---|
| `UsuarioDto` | Respuesta al cliente con datos de un usuario |
| `CreateUsuarioDto` | Cuerpo del request para crear un usuario |
| `UpdateUsuarioDto` | Cuerpo del request para actualizar un usuario |
| `TareaDto` | Respuesta al cliente con datos de una tarea (incluye `TotalAsignaciones`) |
| `CreateTareaDto` | Cuerpo del request para crear una tarea |
| `UpdateTareaDto` | Cuerpo del request para actualizar una tarea |
| `AsignarTareaDto` | Cuerpo del request para asignar una tarea a un usuario (`UsuarioId` + `TareaId`) |
| `TareaAsignadaDto` | Tarea con su estado de asignación, usada en respuestas de `GET /user/{id}` |
| `UsuarioConTareasDto` | Usuario con su lista de tareas asignadas (disponible para extensiones futuras) |

**Validaciones en DTOs** (via Data Annotations):
- `[Required]` — campo obligatorio
- `[StringLength(max, MinimumLength = min)]` — longitud controlada
- `[EmailAddress]` — formato de correo válido

---

## 6. Servicios

Ubicados en `Services/`. Contienen toda la lógica de negocio. Los controllers solo orquestan; los servicios deciden.

### `IUsuarioService` / `UsuarioService`

| Método | Qué hace |
|---|---|
| `GetAllUsuariosAsync()` | Retorna todos los usuarios mapeados a `UsuarioDto` |
| `GetUsuarioByIdAsync(id)` | Busca por ID, retorna `null` si no existe |
| `CreateUsuarioAsync(dto)` | Valida correo único, crea y persiste el usuario |
| `UpdateUsuarioAsync(id, dto)` | Valida que el correo no esté en uso por otro usuario, actualiza |
| `DeleteUsuarioAsync(id)` | Elimina el usuario (cascade elimina sus asignaciones) |
| `UsuarioExisteAsync(id)` | Verificación rápida de existencia |

### `ITareaService` / `TareaService`

| Método | Qué hace |
|---|---|
| `GetAllTareasAsync()` | Retorna todas las tareas con `AsNoTracking` para mejor rendimiento |
| `GetTareaByIdAsync(id)` | Incluye `UsuarioTareas` para calcular `TotalAsignaciones` |
| `CreateTareaAsync(dto)` | Crea y persiste la tarea |
| `UpdateTareaAsync(id, dto)` | Lanza `KeyNotFoundException` si no existe |
| `DeleteTareaAsync(id)` | Elimina la tarea (cascade elimina sus asignaciones) |
| `AsignarTareaAsync(usuarioId, tareaId)` | Verifica existencia de ambos, verifica duplicado, crea el registro en `UsuarioTarea` |
| `DesasignarTareaAsync(usuarioId, tareaId)` | Elimina el registro de `UsuarioTarea` sin afectar usuario ni tarea |
| `GetTareasPorUsuarioAsync(usuarioId)` | Consulta `UsuarioTareas` con `Include(Tarea)` y proyecta a `TareaAsignadaDto` |

### `IMapperService` / `MapperService`

Servicio dedicado al mapeo entre entidades y DTOs para usuarios. Centraliza la transformación y evita duplicar lógica de mapeo en múltiples lugares.

---

## 7. Controllers

Ubicados en `Controllers/`. Reciben las peticiones HTTP, validan el `ModelState`, delegan al servicio correspondiente y retornan la respuesta apropiada.

### `UsuariosController` — `/api/Usuarios`

| Método | Ruta | Descripción | Códigos |
|---|---|---|---|
| GET | `/list` | Lista todos los usuarios | 200 |
| GET | `/find/{id}` | Busca usuario por ID | 200, 400, 404 |
| POST | `/guardar` | Crea un nuevo usuario | 201, 400, 409 |
| PUT | `/actualizar/{id}` | Actualiza un usuario completo | 200, 400, 404, 409 |
| DELETE | `/eliminar/{id}` | Elimina un usuario | 204, 400, 404 |

### `TareasController` — `/api/Tareas`

| Método | Ruta | Descripción | Códigos |
|---|---|---|---|
| GET | `/list` | Lista todas las tareas | 200, 400 |
| GET | `/find/{id}` | Busca tarea por ID | 200, 404, 400 |
| POST | `/guardar` | Crea una nueva tarea | 201, 400 |
| PUT | `/actualizar/{id}` | Actualiza una tarea completa | 200, 400, 404 |
| DELETE | `/eliminar/{id}` | Elimina una tarea (cascade) | 204, 404, 400 |
| POST | `/asignar` | Asigna una tarea a un usuario | 200, 400, 404, 409 |
| DELETE | `/desasignar/{usuarioId}/{tareaId}` | Desasigna una tarea de un usuario | 204, 404, 400 |
| GET | `/user/{usuarioId}` | Lista tareas asignadas a un usuario | 200, 400 |

---

## 8. Middleware

### `ExceptionHandlingMiddleware`

Ubicado en `Middleware/`. Captura cualquier excepción no manejada que escape de los controllers y retorna una respuesta JSON estructurada en lugar de un error 500 genérico.

**Mapeo de excepciones:**

| Excepción | HTTP Status | Mensaje |
|---|---|---|
| `ArgumentNullException` | 400 | Parámetros requeridos nulos |
| `DbUpdateException` | 500 | Error al actualizar la base de datos |
| `InvalidOperationException` | 400 | Operación inválida |
| Cualquier otra | 500 | Error interno del servidor |

Se registra en `Program.cs` antes del pipeline de routing para interceptar todo.

---

## 9. Data Seeding

Ubicado en `Data/`. Al iniciar la aplicación, si la base de datos está vacía, se insertan datos de prueba automáticamente. El proceso es idempotente: si ya existen datos, no hace nada.

### Orden de ejecución (`DataSeeder`)

```
1. UsuarioSeeder    → crea 10 usuarios con nombres/correos aleatorios
2. TareaSeeder      → crea 50 tareas con títulos, prioridades y categorías aleatorias
3. UsuarioTareaSeeder → asigna ~5 tareas por usuario (distribuidas aleatoriamente)
```

### `UsuarioSeeder`
Genera 10 usuarios con nombres y apellidos de un pool predefinido, correos únicos y usernames compuestos. Garantiza que no haya correos duplicados en la generación.

### `TareaSeeder`
Genera 50 tareas combinando verbos (`Implementar`, `Refactorizar`, `Optimizar`...) con características (`API REST`, `base de datos`, `seguridad`...). Asigna categorías (`Backend`, `Testing`, `DevOps`...) y fechas de vencimiento aleatorias (80% de probabilidad de tener una).

### `UsuarioTareaSeeder`
Distribuye las 50 tareas entre los 10 usuarios (~5 por usuario). Asigna estados con probabilidades ponderadas: 50% Pendiente, 30% EnProgreso, 15% Completada, 5% Cancelada.

---

## 10. Documentación de la API (Swagger)

Configurado en `Program.cs` con `Swashbuckle.AspNetCore`. Al acceder a la raíz `/`, redirige automáticamente a `/swagger`.

**Características:**
- Título, versión y contacto configurados en `SwaggerDoc`
- Comentarios XML generados automáticamente desde los `/// <summary>` de los controllers (`GenerateDocumentationFile=true` en el `.csproj`)
- Summaries bilingües (español / inglés) en todos los endpoints
- `[ProducesResponseType]` en cada endpoint para documentar los códigos de respuesta posibles

---

## 11. Program.cs — Configuración del Pipeline

```
1. Registrar DbContext con SQL Server
2. Registrar servicios (Scoped): MapperService, UsuarioService, TareaService
3. Registrar Controllers
4. Configurar Swagger/OpenAPI con XML comments
5. Construir la app
6. Registrar ExceptionHandlingMiddleware
7. Redirigir / → /swagger
8. En Development: habilitar Swagger UI
9. UseHttpsRedirection
10. UseAuthorization
11. MapControllers
12. Aplicar migraciones + ejecutar DataSeeder
13. app.Run()
```

---

## 12. Dependencias (NuGet)

| Paquete | Versión | Para qué |
|---|---|---|
| `Microsoft.AspNetCore.OpenApi` | 10.0.7 | Soporte OpenAPI nativo de .NET 10 |
| `Microsoft.EntityFrameworkCore` | 10.0.7 | ORM base |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.7 | Proveedor SQL Server |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.7 | Herramientas de diseño (migraciones) |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.7 | CLI tools (`dotnet ef`) |
| `Swashbuckle.AspNetCore` | 10.1.7 | Swagger UI |
| `Swashbuckle.AspNetCore.Swagger` | 10.1.7 | Generación del JSON de Swagger |

---

## 13. Migraciones

La migración `AddTareasAndUsuarioTareas` (2026-05-06) crea las tres tablas desde cero:
- `Usuario` — tabla base preexistente
- `Tarea` — nueva tabla
- `UsuarioTarea` — tabla pivote con FKs y cascade delete, más índices en `usuario_id` y `tarea_id`

Para crear una nueva migración:
```bash
dotnet ef migrations add NombreMigracion --project WebAPIUser
dotnet ef database update --project WebAPIUser
```

---

## 14. Estructura de Carpetas

```
WebAPIUser/
├── Controllers/          # Endpoints HTTP
│   ├── UsuariosController.cs
│   └── TareasController.cs
├── Data/                 # Seeding de datos de prueba
│   ├── DataSeeder.cs
│   ├── UsuarioSeeder.cs
│   ├── TareaSeeder.cs
│   └── UsuarioTareaSeeder.cs
├── DTOs/                 # Objetos de transferencia de datos
│   ├── UsuarioDto.cs
│   ├── CreateUsuarioDto.cs
│   ├── UpdateUsuarioDto.cs
│   ├── TareaDto.cs
│   ├── CreateTareaDto.cs
│   ├── UpdateTareaDto.cs
│   ├── AsignarTareaDto.cs
│   └── UsuarioConTareasDto.cs
├── Middleware/           # Manejo global de excepciones
│   └── ExceptionHandlingMiddleware.cs
├── Migrations/           # Historial de cambios de esquema DB
│   └── 20260506180049_AddTareasAndUsuarioTareas.cs
├── Models/               # Entidades EF Core
│   ├── Usuario.cs
│   ├── Tarea.cs
│   ├── UsuarioTarea.cs
│   └── DbUserContext.cs
├── Services/             # Lógica de negocio
│   ├── IUsuarioService / UsuarioService.cs
│   ├── ITareaService.cs
│   ├── TareaService.cs
│   └── MapperService.cs
├── Program.cs            # Entry point y configuración
├── appsettings.json      # Configuración (connection string, logging)
└── WebAPIUser.csproj     # Proyecto y dependencias
```

---

## 15. Decisiones de Diseño

**¿Por qué DTOs separados para Create y Update?**
Permite que cada operación tenga sus propias validaciones sin contaminar el modelo de base de datos ni el DTO de respuesta.

**¿Por qué `AsNoTracking` en las consultas de lectura?**
Las consultas de solo lectura no necesitan que EF Core rastree cambios en los objetos. Mejora el rendimiento al evitar overhead del change tracker.

**¿Por qué `MapperService` en lugar de AutoMapper?**
Para mantener cero dependencias externas de mapeo y tener control total sobre la transformación. El mapeo de usuarios es simple y no justifica una librería adicional.

**¿Por qué cascade delete en `UsuarioTarea`?**
Garantiza integridad referencial automáticamente. Al eliminar un usuario o tarea, no quedan registros huérfanos en la tabla pivote.

**¿Por qué el seeder es idempotente?**
Permite reiniciar la aplicación sin duplicar datos. Verifica existencia antes de insertar.
