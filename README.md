# WebAPIUser

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-purple)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0.7-orange)](https://learn.microsoft.com/en-us/ef/core/)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI%20v1-brightgreen)](https://swagger.io/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-red)](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

API REST construida con .NET 10 para la gestión de **usuarios** y **tareas**, con soporte de asignación muchos a muchos (M:M) entre ambas entidades. Incluye documentación interactiva con Swagger, seeding automático de datos de prueba y middleware global de manejo de errores.

---

## Tabla de contenidos

- [Stack](#stack)
- [Requisitos](#requisitos)
- [Instalación y ejecución](#instalación-y-ejecución)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Base de datos](#base-de-datos)
- [Endpoints](#endpoints)
- [Ejemplos de uso](#ejemplos-de-uso)
- [Arquitectura](#arquitectura)
- [Documentación adicional](#documentación-adicional)

---

## Stack

| Tecnología | Versión | Uso |
|---|---|---|
| .NET / ASP.NET Core | 10.0 | Framework principal |
| C# | 14.0 | Lenguaje |
| Entity Framework Core | 10.0.7 | ORM y migraciones |
| SQL Server LocalDB | — | Base de datos |
| Swashbuckle (Swagger) | 10.1.7 | Documentación interactiva |
| Microsoft.AspNetCore.OpenApi | 10.0.7 | Soporte OpenAPI nativo |

---

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (incluido con Visual Studio 2022) o SQL Server Express
- Visual Studio 2022 / VS Code / Rider

---

## Instalación y ejecución

```bash
# 1. Clonar el repositorio
git clone https://github.com/LibardoOrtega2022/WebAPIUser.git
cd WebAPIUser/WebAPIUser

# 2. Restaurar dependencias
dotnet restore

# 3. Ejecutar (las migraciones y el seeding se aplican automáticamente al arrancar)
dotnet run
```

Al iniciar, la aplicación:
1. Aplica las migraciones pendientes con `MigrateAsync()`.
2. Inserta datos de prueba si las tablas están vacías (10 usuarios, 50 tareas, ~50 asignaciones).
3. Redirige la raíz `/` a `/swagger`.

Swagger UI disponible en: `http://localhost:5000/swagger`

### Cadena de conexión

Configurada en `appsettings.json`:

```json
"ConnectionStrings": {
  "connectionDB": "Data Source=(localdb)\\MSSQLLocalDB;Database=db_user;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Para usar otra instancia de SQL Server, reemplaza el valor de `connectionDB`.

---

## Estructura del proyecto

```
WebAPIUser/
├── Controllers/
│   ├── UsuariosController.cs     — CRUD de usuarios
│   └── TareasController.cs       — CRUD de tareas + asignaciones M:M
│
├── Data/
│   ├── DataSeeder.cs             — Orquestador del seeding
│   ├── UsuarioSeeder.cs          — Genera 10 usuarios de prueba
│   ├── TareaSeeder.cs            — Genera 50 tareas de prueba
│   └── UsuarioTareaSeeder.cs     — Asigna tareas a usuarios
│
├── DTOs/
│   ├── UsuarioDto.cs             — Respuesta de usuario
│   ├── CreateUsuarioDto.cs       — Entrada para crear usuario
│   ├── UpdateUsuarioDto.cs       — Entrada para actualizar usuario
│   ├── TareaDto.cs               — Respuesta de tarea
│   ├── CreateTareaDto.cs         — Entrada para crear tarea
│   ├── UpdateTareaDto.cs         — Entrada para actualizar tarea
│   ├── AsignarTareaDto.cs        — Entrada para asignar tarea a usuario
│   └── UsuarioConTareasDto.cs    — Usuario con lista de tareas asignadas
│
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs  — Manejo global de excepciones
│
├── Migrations/
│   └── 20260506180049_AddTareasAndUsuarioTareas.cs
│
├── Models/
│   ├── Usuario.cs                — Entidad usuario
│   ├── Tarea.cs                  — Entidad tarea + PrioridadEnum
│   ├── UsuarioTarea.cs           — Tabla pivote M:M + EstadoTareaEnum
│   └── DbUserContext.cs          — DbContext con Fluent API
│
├── Services/
│   ├── IUsuarioService.cs / UsuarioService.cs
│   ├── ITareaService.cs          — Interfaz del servicio de tareas
│   ├── TareaService.cs           — Lógica de negocio de tareas
│   └── MapperService.cs          — Mapeo entidad ↔ DTO para usuarios
│
├── Documentacion/
│   └── PROJECT_DOCUMENTATION.md  — Documentación técnica completa
│
├── Program.cs                    — Entry point y configuración del pipeline
├── appsettings.json              — Configuración (connection string, logging)
└── WebAPIUser.csproj             — Dependencias NuGet
```

---

## Base de datos

La base de datos `db_user` se crea automáticamente en LocalDB al ejecutar la aplicación.

### Diagrama de tablas

```
┌──────────────────┐        ┌──────────────────────┐        ┌──────────────────┐
│     Usuario      │        │    UsuarioTarea       │        │      Tarea       │
├──────────────────┤        │    (Tabla Pivote)     │        ├──────────────────┤
│ id          PK   │◄──────►│ id              PK   │◄──────►│ id          PK   │
│ nombres          │        │ usuario_id      FK   │        │ titulo           │
│ apellidos        │        │ tarea_id        FK   │        │ descripcion      │
│ correo           │        │ fecha_asignacion     │        │ prioridad        │
│ username         │        │ estado               │        │ categoria        │
│ fecha_creacion   │        │ fecha_completado      │        │ fecha_vencimiento│
└──────────────────┘        └──────────────────────┘        │ fecha_creacion   │
                                                             └──────────────────┘
```

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

### Cascade delete

Al eliminar un `Usuario` o una `Tarea`, todos sus registros en `UsuarioTarea` se eliminan automáticamente.

---

## Endpoints

### Usuarios — `/api/Usuarios`

| Método | Ruta | Descripción | Códigos |
|---|---|---|---|
| GET | `/api/Usuarios/list` | Lista todos los usuarios | 200 |
| GET | `/api/Usuarios/find/{id}` | Busca usuario por ID | 200, 400, 404 |
| POST | `/api/Usuarios/guardar` | Crea un nuevo usuario | 201, 400, 409 |
| PUT | `/api/Usuarios/actualizar/{id}` | Actualiza un usuario | 200, 400, 404, 409 |
| DELETE | `/api/Usuarios/eliminar/{id}` | Elimina un usuario | 204, 400, 404 |

### Tareas — `/api/Tareas`

| Método | Ruta | Descripción | Códigos |
|---|---|---|---|
| GET | `/api/Tareas/list` | Lista todas las tareas | 200, 400 |
| GET | `/api/Tareas/find/{id}` | Busca tarea por ID | 200, 400, 404 |
| POST | `/api/Tareas/guardar` | Crea una nueva tarea | 201, 400 |
| PUT | `/api/Tareas/actualizar/{id}` | Actualiza una tarea | 200, 400, 404 |
| DELETE | `/api/Tareas/eliminar/{id}` | Elimina una tarea (cascade) | 204, 400, 404 |
| POST | `/api/Tareas/asignar` | Asigna una tarea a un usuario | 200, 400, 404, 409 |
| DELETE | `/api/Tareas/desasignar/{usuarioId}/{tareaId}` | Desasigna una tarea de un usuario | 204, 400, 404 |
| GET | `/api/Tareas/user/{usuarioId}` | Lista tareas asignadas a un usuario | 200, 400 |

---

## Ejemplos de uso

### Crear un usuario

```bash
curl -X POST http://localhost:5000/api/Usuarios/guardar \
  -H "Content-Type: application/json" \
  -d '{
    "nombres": "Juan",
    "apellidos": "García",
    "correo": "juan.garcia@example.com",
    "username": "juangarcia"
  }'
```

**Respuesta 201 Created:**
```json
{
  "id": 1,
  "nombres": "Juan",
  "apellidos": "García",
  "correo": "juan.garcia@example.com",
  "username": "juangarcia",
  "fechaCreacion": "2026-05-08T14:30:00Z"
}
```

### Crear una tarea

```bash
curl -X POST http://localhost:5000/api/Tareas/guardar \
  -H "Content-Type: application/json" \
  -d '{
    "titulo": "Implementar autenticación JWT",
    "descripcion": "Agregar login con tokens JWT al proyecto",
    "prioridad": 3,
    "categoria": "Seguridad",
    "fechaVencimiento": "2026-06-01T00:00:00Z"
  }'
```

**Respuesta 201 Created:**
```json
{
  "success": true,
  "message": "Tarea creada exitosamente",
  "data": {
    "id": 1,
    "titulo": "Implementar autenticación JWT",
    "descripcion": "Agregar login con tokens JWT al proyecto",
    "prioridad": 3,
    "categoria": "Seguridad",
    "fechaVencimiento": "2026-06-01T00:00:00Z",
    "fechaCreacion": "2026-05-08T14:30:00Z",
    "totalAsignaciones": 0
  },
  "timestamp": "2026-05-08T14:30:00Z"
}
```

### Asignar una tarea a un usuario

```bash
curl -X POST http://localhost:5000/api/Tareas/asignar \
  -H "Content-Type: application/json" \
  -d '{
    "usuarioId": 1,
    "tareaId": 1
  }'
```

**Respuesta 200 OK:**
```json
{
  "success": true,
  "message": "Tarea asignada exitosamente",
  "timestamp": "2026-05-08T14:31:00Z"
}
```

### Ver tareas de un usuario

```bash
curl http://localhost:5000/api/Tareas/user/1
```

**Respuesta 200 OK:**
```json
{
  "success": true,
  "message": "Se encontraron 3 tareas para el usuario",
  "data": [
    {
      "tareaId": 1,
      "titulo": "Implementar autenticación JWT",
      "descripcion": "Agregar login con tokens JWT al proyecto",
      "prioridad": 3,
      "estadoAsignacion": 1,
      "fechaAsignacion": "2026-05-08T14:31:00Z",
      "fechaCompletado": null
    }
  ],
  "timestamp": "2026-05-08T14:32:00Z"
}
```

### Desasignar una tarea

```bash
curl -X DELETE http://localhost:5000/api/Tareas/desasignar/1/1
```

**Respuesta 204 No Content**

---

## Arquitectura

El flujo de una petición siempre sigue el mismo camino:

```
Request HTTP
    │
    ▼
ExceptionHandlingMiddleware   ← captura cualquier excepción no manejada
    │
    ▼
Controller                    ← valida ModelState, delega al servicio
    │
    ▼
Service                       ← lógica de negocio, validaciones de dominio
    │
    ▼
DbUserContext (EF Core)       ← traduce objetos C# a SQL
    │
    ▼
SQL Server (db_user)
```

### Capas

- **Controllers** — reciben la petición HTTP, validan el modelo y retornan la respuesta. No contienen lógica de negocio.
- **Services** — contienen toda la lógica: validación de correo único, verificación de existencia, mapeo de entidades.
- **MapperService** — centraliza la transformación entre entidades `Usuario` y sus DTOs.
- **DbUserContext** — configura el mapeo columna-propiedad con Fluent API (nombres en snake_case, tipos explícitos, relaciones con cascade delete).
- **Middleware** — `ExceptionHandlingMiddleware` intercepta excepciones y retorna JSON estructurado con el código HTTP apropiado.
- **Data Seeders** — insertan datos de prueba al arrancar si las tablas están vacías. Son idempotentes.

### Decisiones de diseño

- `AsNoTracking()` en todas las consultas de solo lectura para mejor rendimiento.
- DTOs separados para Create/Update/Response — cada operación tiene sus propias validaciones sin contaminar el modelo.
- Cascade delete en `UsuarioTarea` — al eliminar un usuario o tarea, sus asignaciones se eliminan automáticamente sin código adicional.
- El seeder es idempotente — reiniciar la app no duplica datos.

---

## Documentación adicional

Dentro de `WebAPIUser/Documentacion/`:

| Archivo | Contenido |
|---|---|
| `PROJECT_DOCUMENTATION.md` | Documentación técnica completa: DB, modelos, DTOs, servicios, endpoints, decisiones de diseño |

La documentación XML de todos los métodos, clases y propiedades está generada automáticamente desde los `/// <summary>` del código y es visible en Swagger UI.

---

## Comandos útiles

### Migraciones

```bash
# Crear una nueva migración
dotnet ef migrations add NombreMigracion --project WebAPIUser

# Aplicar migraciones pendientes
dotnet ef database update --project WebAPIUser

# Ver el historial de migraciones
dotnet ef migrations list --project WebAPIUser

# Revertir a una migración anterior
dotnet ef database update NombreMigracionAnterior --project WebAPIUser

# Eliminar la última migración (solo si no fue aplicada a la DB)
dotnet ef migrations remove --project WebAPIUser
```

### Build y ejecución

```bash
# Restaurar dependencias
dotnet restore

# Compilar sin ejecutar
dotnet build

# Ejecutar en modo desarrollo (aplica migraciones + seeding automático)
dotnet run

# Ejecutar en modo producción
dotnet run --environment Production

# Publicar para despliegue
dotnet publish -c Release -o ./publish
```

### Verificar la base de datos

```bash
# Conectar a LocalDB desde línea de comandos
sqlcmd -S "(localdb)\MSSQLLocalDB" -d db_user -Q "SELECT * FROM Usuario"
sqlcmd -S "(localdb)\MSSQLLocalDB" -d db_user -Q "SELECT * FROM Tarea"
sqlcmd -S "(localdb)\MSSQLLocalDB" -d db_user -Q "SELECT * FROM UsuarioTarea"
```

---

## Probar la API

### Con Swagger UI

Con el proyecto corriendo, abre en el navegador:

```
http://localhost:5000/swagger
```

Desde ahí puedes ejecutar cualquier endpoint directamente sin herramientas externas.

### Con curl

```bash
# Listar todos los usuarios
curl http://localhost:5000/api/Usuarios/list

# Buscar usuario por ID
curl http://localhost:5000/api/Usuarios/find/1

# Crear usuario
curl -X POST http://localhost:5000/api/Usuarios/guardar \
  -H "Content-Type: application/json" \
  -d '{"nombres":"Ana","apellidos":"López","correo":"ana.lopez@example.com","username":"analopez"}'

# Actualizar usuario
curl -X PUT http://localhost:5000/api/Usuarios/actualizar/1 \
  -H "Content-Type: application/json" \
  -d '{"nombres":"Ana","apellidos":"López","correo":"ana.nuevo@example.com","username":"analopez"}'

# Eliminar usuario
curl -X DELETE http://localhost:5000/api/Usuarios/eliminar/1

# Listar todas las tareas
curl http://localhost:5000/api/Tareas/list

# Buscar tarea por ID
curl http://localhost:5000/api/Tareas/find/1

# Crear tarea
curl -X POST http://localhost:5000/api/Tareas/guardar \
  -H "Content-Type: application/json" \
  -d '{"titulo":"Revisar código","descripcion":"Code review del sprint","prioridad":2,"categoria":"Desarrollo"}'

# Asignar tarea a usuario
curl -X POST http://localhost:5000/api/Tareas/asignar \
  -H "Content-Type: application/json" \
  -d '{"usuarioId":1,"tareaId":1}'

# Ver tareas de un usuario
curl http://localhost:5000/api/Tareas/user/1

# Desasignar tarea de usuario
curl -X DELETE http://localhost:5000/api/Tareas/desasignar/1/1
```

### Con Postman

1. Importa una nueva colección.
2. Establece la variable de entorno `base_url = http://localhost:5000`.
3. Usa los endpoints de la tabla de la sección [Endpoints](#endpoints).
4. Para POST y PUT, selecciona `Body → raw → JSON` y pega el cuerpo del request.

---

## Solución de problemas frecuentes

**La app no arranca y dice que no puede conectar a la base de datos**
- Verifica que SQL Server LocalDB esté instalado: `sqllocaldb info`
- Si no existe la instancia, créala: `sqllocaldb create MSSQLLocalDB`
- Inicia la instancia: `sqllocaldb start MSSQLLocalDB`

**Error "Cannot open database db_user"**
- Las migraciones se aplican automáticamente al arrancar. Si falla, ejecuta manualmente:
  ```bash
  dotnet ef database update --project WebAPIUser
  ```

**El puerto 5000 ya está en uso**
- Cambia el puerto en `Properties/launchSettings.json` o usa:
  ```bash
  dotnet run --urls "http://localhost:5001"
  ```

**Swagger no carga los comentarios XML**
- Verifica que `<GenerateDocumentationFile>true</GenerateDocumentationFile>` esté en el `.csproj`.
- Recompila el proyecto: `dotnet build`.

**Los datos de prueba no se insertan**
- El seeder solo inserta si las tablas están vacías. Si quieres reiniciar los datos:
  ```bash
  # Borrar y recrear la base de datos
  dotnet ef database drop --project WebAPIUser
  dotnet ef database update --project WebAPIUser
  # Luego ejecuta la app normalmente
  dotnet run
  ```

---

## Contribuir

1. Haz fork del repositorio
2. Crea una rama: `git checkout -b feature/nombre-feature`
3. Haz commit de tus cambios: `git commit -m 'feat: descripción'`
4. Push a la rama: `git push origin feature/nombre-feature`
5. Abre un Pull Request

---

**Autor:** Libardo Amesquita · `libardoadolfo2@gmail.com`
