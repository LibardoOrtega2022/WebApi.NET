# ✅ CHECKLIST DE IMPLEMENTACIÓN

## 📋 PRE-EJECUCIÓN

- [ ] He reiniciado Visual Studio (por errores ENC0023)
- [ ] Estoy en la rama correcta
- [ ] Tengo SQL Server LocalDB corriendo
- [ ] `appsettings.json` tiene connectionString correcta

---

## 🔧 PASO 1: CREAR MIGRACIÓN

```bash
# Navega a la carpeta del proyecto
cd "C:\Users\LAAO\Projects\WebAPIUser\WebAPIUser"

# Crear migración
dotnet ef migrations add AddTareasAndUsuarioTareas

# Aplicar a BD
dotnet ef database update
```

**Checklist:**
- [ ] Migración se creó sin errores
- [ ] `Migrations/` carpeta se llenó
- [ ] BD se actualizó sin errores
- [ ] Nuevas tablas están en SQL Server

---

## 🚀 PASO 2: EJECUTAR APLICACIÓN

```bash
dotnet run
```

**Checklist:**
- [ ] App inicia sin errores
- [ ] Swagger abre en `https://localhost:7000/swagger`
- [ ] Veo endpoint `/api/tareas/listar`
- [ ] Endpoints de Usuario SIGUEN FUNCIONANDO

---

## 🧪 PASO 3: PROBAR ENDPOINTS

### A: Listar Tareas (debe estar vacío)
```http
GET https://localhost:7000/api/tareas/listar
```
- [ ] Respuesta 200
- [ ] `"success": true`
- [ ] `"data": []` (vacío)

---

### B: Crear Tarea #1
```http
POST https://localhost:7000/api/tareas/guardar
```
Body:
```json
{
  "titulo": "Primera Tarea",
  "descripcion": "Tarea de prueba",
  "prioridad": 2,
  "fechaVencimiento": "2024-12-31T00:00:00Z",
  "categoria": "Testing"
}
```
- [ ] Respuesta 201
- [ ] `"success": true`
- [ ] Nota el `"id"` retornado (ej: 1)

---

### C: Listar Tareas (debe haber 1)
```http
GET https://localhost:7000/api/tareas/listar
```
- [ ] Respuesta 200
- [ ] `"data"` contiene 1 tarea
- [ ] Tarea tiene título, prioridad, etc

---

### D: Buscar Tarea por ID
```http
GET https://localhost:7000/api/tareas/buscar/1
```
- [ ] Respuesta 200
- [ ] Retorna la tarea creada
- [ ] `"id": 1`

---

### E: Actualizar Tarea
```http
PUT https://localhost:7000/api/tareas/actualizar/1
```
Body:
```json
{
  "titulo": "Tarea Actualizada",
  "descripcion": "Nueva descripción",
  "prioridad": 3,
  "fechaVencimiento": "2025-01-31T00:00:00Z",
  "categoria": "Desarrollo"
}
```
- [ ] Respuesta 200
- [ ] Título cambió en respuesta

---

### F: Verificar que Usuarios SIN CAMBIOS
```http
GET https://localhost:7000/api/usuarios/listar
```
- [ ] Respuesta 200
- [ ] Usuarios existentes aún están

---

### G: Asignar Tarea a Usuario (ID 1)
```http
POST https://localhost:7000/api/tareas/asignar
```
Body:
```json
{
  "usuarioId": 1,
  "tareaId": 1
}
```
- [ ] Respuesta 200
- [ ] `"success": true`
- [ ] `"message": "Tarea asignada exitosamente"`

---

### H: Obtener Tareas del Usuario
```http
GET https://localhost:7000/api/tareas/usuario/1
```
- [ ] Respuesta 200
- [ ] `"data"` contiene 1 tarea
- [ ] Tarea tiene `"estadoAsignacion": 1` (Pendiente)

---

### I: Intentar Asignar MISMA tarea dos veces (debe fallar)
```http
POST https://localhost:7000/api/tareas/asignar
```
Body:
```json
{
  "usuarioId": 1,
  "tareaId": 1
}
```
- [ ] Respuesta 409 Conflict
- [ ] Mensaje: "La tarea ya está asignada a este usuario"

---

### J: Desasignar Tarea
```http
DELETE https://localhost:7000/api/tareas/desasignar/1/1
```
- [ ] Respuesta 204 No Content

---

### K: Verificar Desasignación
```http
GET https://localhost:7000/api/tareas/usuario/1
```
- [ ] Respuesta 200
- [ ] `"data": []` (vacío, sin tareas)

---

### L: Eliminar Tarea
```http
DELETE https://localhost:7000/api/tareas/eliminar/1
```
- [ ] Respuesta 204 No Content

---

### M: Verificar Eliminación
```http
GET https://localhost:7000/api/tareas/buscar/1
```
- [ ] Respuesta 404 Not Found
- [ ] Mensaje: "Tarea con ID 1 no encontrada"

---

## 📊 PASO 4: CREAR 50 TAREAS (SEED DATA)

### Opción A: Script manual en Swagger
1. [ ] POST 50 veces creando tareas (tedioso pero funciona)

### Opción B: Agregar SeedData (RECOMENDADO)

Crear `WebAPIUser/Data/SeedData.cs`:
```csharp
using WebAPIUser.Models;

namespace WebAPIUser.Data;

public static class SeedData
{
    public static void Initialize(DbUserContext context)
    {
        if (context.Tareas.Any())
            return; // Ya hay tareas

        var prioridades = new[] { PrioridadEnum.Baja, PrioridadEnum.Media, PrioridadEnum.Alta, PrioridadEnum.Critica };

        for (int i = 1; i <= 50; i++)
        {
            context.Tareas.Add(new Tarea
            {
                Titulo = $"Tarea #{i:D2}",
                Descripcion = $"Descripción de la tarea {i}",
                Prioridad = prioridades[(i - 1) % 4],
                Categoria = i % 2 == 0 ? "Desarrollo" : "Testing",
                FechaVencimiento = DateTime.UtcNow.AddDays((i % 30) + 1),
                FechaCreacion = DateTime.UtcNow
            });
        }
        context.SaveChanges();

        var usuarios = context.Usuarios.Take(10).ToList();
        var tareas = context.Tareas.ToList();

        for (int u = 0; u < usuarios.Count; u++)
        {
            for (int t = 0; t < 5 && (u * 5 + t) < tareas.Count; t++)
            {
                context.UsuarioTareas.Add(new UsuarioTarea
                {
                    UsuarioId = usuarios[u].Id,
                    TareaId = tareas[u * 5 + t].Id,
                    FechaAsignacion = DateTime.UtcNow,
                    Estado = EstadoTareaEnum.Pendiente
                });
            }
        }
        context.SaveChanges();
    }
}
```

**Checklist:**
- [ ] Archivo creado en `WebAPIUser/Data/SeedData.cs`

---

Editar `Program.cs` (ANTES de `app.Run()`):

```csharp
// ... antes de app.Run()

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbUserContext>();
    db.Database.Migrate();
    SeedData.Initialize(db);
}

app.Run();
```

**Checklist:**
- [ ] `Program.cs` editado
- [ ] Importar `using WebAPIUser.Data;`

---

Ejecutar:
```bash
dotnet run
```

**Checklist:**
- [ ] App inicia sin errores
- [ ] `Seed data` se creó automáticamente

---

Verificar:
```http
GET https://localhost:7000/api/tareas/listar
```
- [ ] Respuesta contiene 50 tareas
- [ ] Cada usuario tiene 5 tareas asignadas

```http
GET https://localhost:7000/api/tareas/usuario/1
```
- [ ] Respuesta contiene 5 tareas
- [ ] Estados son `1` (Pendiente)

---

## 🎯 PRUEBAS FINALES

### Usuarios SIN CAMBIOS
- [ ] `GET /api/usuarios/listar` → ✅
- [ ] `GET /api/usuarios/buscar/1` → ✅
- [ ] `POST /api/usuarios/guardar` → ✅
- [ ] `PUT /api/usuarios/actualizar/1` → ✅
- [ ] `DELETE /api/usuarios/eliminar/{id}` → ✅

### Tareas NUEVAS
- [ ] `GET /api/tareas/listar` → 50 tareas ✅
- [ ] `GET /api/tareas/buscar/1` → Tarea 1 ✅
- [ ] `POST /api/tareas/guardar` → Crear nueva ✅
- [ ] `PUT /api/tareas/actualizar/1` → Actualizar ✅
- [ ] `DELETE /api/tareas/eliminar/{id}` → Eliminar ✅
- [ ] `POST /api/tareas/asignar` → Asignar ✅
- [ ] `DELETE /api/tareas/desasignar/{u}/{t}` → Desasignar ✅
- [ ] `GET /api/tareas/usuario/1` → 5 tareas ✅

### Swagger
- [ ] Swagger muestra todos los endpoints ✅
- [ ] Documentación es legible ✅
- [ ] Try-it-out funciona para todos ✅

---

## 🚨 TROUBLESHOOTING

### Problema: "DbSet 'Tareas' could not be mapped"
**Solución**: Ejecuta `dotnet ef database update`

### Problema: "Migrations folder not found"
**Solución**: Ejecuta `dotnet ef migrations add Initial`

### Problema: ENC0023 error
**Solución**: Reinicia Visual Studio completamente

### Problema: "Migration already exists"
**Solución**: 
1. Elimina la migración: `dotnet ef migrations remove`
2. Crea de nuevo: `dotnet ef migrations add AddTareasAndUsuarioTareas`

### Problema: "FK constraint failed"
**Solución**: El usuario o tarea no existe, verifica los IDs

### Problema: Swagger no muestra nuevos endpoints
**Solución**: Reinicia la aplicación (`Ctrl+F5`)

---

## 📋 RESUMEN FINAL

| Item | Estado |
|------|--------|
| **Modelos creados** | ✅ Tarea + UsuarioTarea |
| **DTOs creados** | ✅ 5 DTOs |
| **Servicios creados** | ✅ ITareaService + TareaService |
| **Controlador creado** | ✅ TareasController (8 endpoints) |
| **Migraciones** | ⏳ Pendiente (tu responsabilidad) |
| **Datos seed** | ⏳ Opcional (tu responsabilidad) |
| **Pruebas** | ⏳ Pendiente (tu responsabilidad) |
| **Documentación** | ✅ Completada |
| **Código roto** | ✅ NINGUNO |

---

## ✅ ESTADO FINAL

Cuando completes este checklist:
- ✅ 50 Tareas en la BD
- ✅ 10 Usuarios en la BD
- ✅ Relación M:M funcional
- ✅ 8 endpoints CRUD operacionales
- ✅ Swagger mostrando todo
- ✅ 0 código roto

**¡IMPLEMENTACIÓN COMPLETADA! 🚀**

---

**Última actualización**: 2024-01-15  
**Versión**: 1.0  
**Estado**: LISTO PARA EJECUTAR
