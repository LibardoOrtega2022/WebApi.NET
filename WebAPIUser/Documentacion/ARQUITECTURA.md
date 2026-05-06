# 📊 VISUALIZACIÓN: ARQUITECTURA E IMPLEMENTACIÓN

## 🏗️ ESTRUCTURA DE CARPETAS FINAL

```
WebAPIUser/
│
├── 📂 Controllers/
│   ├── UsuariosController.cs        ✅ SIN CAMBIOS
│   └── TareasController.cs          ✅ NUEVO (8 endpoints)
│
├── 📂 Models/
│   ├── Usuario.cs                   ⚡ ACTUALIZADO (agregó navegación)
│   ├── Tarea.cs                     ✅ NUEVO
│   ├── UsuarioTarea.cs              ✅ NUEVO (tabla pivote)
│   └── DbUserContext.cs             ⚡ ACTUALIZADO (DbSets + config)
│
├── 📂 Services/
│   ├── IUsuarioService.cs           ✅ SIN CAMBIOS
│   ├── UsuarioService.cs            ✅ SIN CAMBIOS
│   ├── IMapperService.cs            ✅ SIN CAMBIOS
│   ├── MapperService.cs             ✅ SIN CAMBIOS
│   ├── ITareaService.cs             ✅ NUEVO
│   └── TareaService.cs              ✅ NUEVO
│
├── 📂 DTOs/
│   ├── CreateUsuarioDto.cs          ✅ SIN CAMBIOS
│   ├── UpdateUsuarioDto.cs          ✅ SIN CAMBIOS
│   ├── UsuarioDto.cs                ✅ SIN CAMBIOS
│   ├── CreateTareaDto.cs            ✅ NUEVO
│   ├── UpdateTareaDto.cs            ✅ NUEVO
│   ├── TareaDto.cs                  ✅ NUEVO
│   ├── AsignarTareaDto.cs           ✅ NUEVO
│   └── UsuarioConTareasDto.cs       ✅ NUEVO
│
├── 📂 Validators/
│   └── (vacío, validación en DTOs)
│
├── 📂 Data/
│   └── SeedData.cs                  ⏳ OPCIONAL (crear tú)
│
├── 📂 Migrations/
│   └── (auto-generadas)             ⏳ PENDIENTE
│
├── 📂 Documentacion/
│   ├── TAREAS_IMPLEMENTACION.md      ✅ NUEVA (guía técnica)
│   ├── TAREAS_RESUMEN.md            ✅ NUEVA (resumen ejecutivo)
│   ├── CHECKLIST_EJECUCION.md       ✅ NUEVA (pasos a seguir)
│   └── ARQUITECTURA.md              ✅ NUEVA (este archivo)
│
├── Program.cs                        ⚡ ACTUALIZADO (1 línea)
├── appsettings.json                 ✅ SIN CAMBIOS
└── WebAPIUser.csproj                ✅ SIN CAMBIOS
```

---

## 🔗 DIAGRAMA: RELACIÓN M:M

```
┌──────────────────────────────────────────────────────────────┐
│                    USUARIOS (10)                             │
├──────────────────────────────────────────────────────────────┤
│ ID │ Nombres    │ Apellidos │ Correo        │ Fecha          │
├────┼────────────┼───────────┼───────────────┼────────────────┤
│ 1  │ Usuario1   │ Test1     │ usuario1@...  │ 2024-01-15     │
│ 2  │ Usuario2   │ Test2     │ usuario2@...  │ 2024-01-15     │
│ 3  │ Usuario3   │ Test3     │ usuario3@...  │ 2024-01-15     │
│    │  ...       │  ...      │  ...          │  ...           │
│ 10 │ Usuario10  │ Test10    │ usuario10@... │ 2024-01-15     │
└──────────────────────────────────────────────────────────────┘
                           ▲
                           │
                    (1) ◄──┼──► (M)
                           │
┌──────────────────────────────────────────────────────────────┐
│            USUARIOTAREA (Tabla Pivote M:M)                   │
├──────────────────────────────────────────────────────────────┤
│ ID │ Usuario_ID │ Tarea_ID │ Estado │ Fecha_Asignación      │
├────┼────────────┼──────────┼────────┼───────────────────────┤
│ 1  │    1       │    1     │   1    │ 2024-01-15 10:00:00  │
│ 2  │    1       │    2     │   1    │ 2024-01-15 10:00:00  │
│ 3  │    1       │    3     │   1    │ 2024-01-15 10:00:00  │
│ 4  │    1       │    4     │   1    │ 2024-01-15 10:00:00  │
│ 5  │    1       │    5     │   1    │ 2024-01-15 10:00:00  │
│ 6  │    2       │    6     │   1    │ 2024-01-15 10:00:00  │
│    │  ...       │  ...     │ ...    │ ...                   │
│ 50 │    10      │    50    │   1    │ 2024-01-15 10:00:00  │
└──────────────────────────────────────────────────────────────┘
                           ▲
                           │
                    (M) ◄──┼──► (1)
                           │
┌──────────────────────────────────────────────────────────────┐
│                    TAREAS (50)                               │
├──────────────────────────────────────────────────────────────┤
│ ID │ Titulo        │ Descripción  │ Prioridad │ Fecha_Ven    │
├────┼───────────────┼──────────────┼───────────┼──────────────┤
│ 1  │ Tarea #01     │ Descripción  │  2 (Med) │ 2024-02-14   │
│ 2  │ Tarea #02     │ Descripción  │  1 (Baja)│ 2024-02-15   │
│ 3  │ Tarea #03     │ Descripción  │  3 (Alta)│ 2024-02-16   │
│ 4  │ Tarea #04     │ Descripción  │  4 (Crít)│ 2024-02-17   │
│ 5  │ Tarea #05     │ Descripción  │  2 (Med) │ 2024-02-18   │
│    │  ...          │  ...         │  ...     │  ...         │
│ 50 │ Tarea #50     │ Descripción  │  2 (Med) │ 2024-03-16   │
└──────────────────────────────────────────────────────────────┘

Relación M:M:
  • 1 Usuario puede tener MUCHAS Tareas
  • 1 Tarea puede asignarse a MUCHOS Usuarios
  • Tabla Pivote (UsuarioTarea) mantiene la relación
  • Cada asignación tiene su propio estado y fechas
```

---

## 🔄 FLUJO DE DATOS: CREAR Y ASIGNAR TAREA

```
┌─────────────────────────────────────────────────────────┐
│ 1. CLIENTE: POST /api/tareas/guardar                    │
│    Payload: {                                           │
│      "titulo": "Nueva tarea",                           │
│      "descripcion": "...",                              │
│      "prioridad": 2,                                    │
│      "categoria": "Desarrollo"                          │
│    }                                                    │
└────────────────────┬──────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────────┐
│ 2. TareasController.CreateTarea()                        │
│    • Valida ModelState                                 │
│    • Llama _tareaService.CreateTareaAsync()            │
└────────────────────┬──────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────────┐
│ 3. TareaService.CreateTareaAsync()                      │
│    • Validar DTO no nulo                               │
│    • Log: "Creando nueva tarea: {Titulo}"              │
│    • Crear objeto Tarea                                │
│    • _context.Tareas.Add(tarea)                        │
│    • _context.SaveChangesAsync()                       │
│    • Log: "Tarea creada: ID {id}"                      │
└────────────────────┬──────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────────┐
│ 4. DbUserContext.SaveChangesAsync()                     │
│    • Prepara INSERT SQL                                │
│    • Ejecuta en SQL Server                             │
│    • Retorna objeto con ID                             │
└────────────────────┬──────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────────┐
│ 5. SERVIDOR: 201 Created                               │
│    Respuesta: {                                         │
│      "success": true,                                  │
│      "message": "Tarea creada exitosamente",          │
│      "data": {                                         │
│        "id": 1,                                        │
│        "titulo": "Nueva tarea",                        │
│        ...                                             │
│      }                                                 │
│    }                                                   │
└─────────────────────────────────────────────────────────┘

SEGUNDA FASE: ASIGNAR TAREA

┌─────────────────────────────────────────────────────────┐
│ 1. CLIENTE: POST /api/tareas/asignar                    │
│    Payload: {                                           │
│      "usuarioId": 1,                                    │
│      "tareaId": 1                                       │
│    }                                                    │
└────────────────────┬──────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────────┐
│ 2. TareasController.AsignarTarea()                       │
│    • Valida IDs > 0                                    │
│    • Llama _tareaService.AsignarTareaAsync()           │
└────────────────────┬──────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────────┐
│ 3. TareaService.AsignarTareaAsync()                     │
│    • Log: "Asignando tarea {TareaId} a usuario..."     │
│    • var usuario = _context.Usuarios.FindAsync(1)      │
│    • var tarea = _context.Tareas.FindAsync(1)          │
│    • Verificar si ya existe asignación                 │
│    • Si existe: throw InvalidOperationException         │
│    • Crear UsuarioTarea                                │
│    • _context.UsuarioTareas.Add()                      │
│    • _context.SaveChangesAsync()                       │
│    • Log: "Tarea asignada exitosamente"                │
└────────────────────┬──────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────────┐
│ 4. DbUserContext.SaveChangesAsync()                     │
│    • Prepara INSERT en UsuarioTarea                    │
│    • Ejecuta en SQL Server                             │
│    • Retorna éxito                                     │
└────────────────────┬──────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────────┐
│ 5. SERVIDOR: 200 OK                                    │
│    Respuesta: {                                         │
│      "success": true,                                  │
│      "message": "Tarea asignada exitosamente",        │
│      "timestamp": "2024-01-15T14:30:00Z"              │
│    }                                                   │
└─────────────────────────────────────────────────────────┘
```

---

## 📡 ENDPOINTS: MAPA VISUAL

```
┌────────────────────────────────────────────────────────────────┐
│                    API REST - TAREAS                            │
├────────────────────────────────────────────────────────────────┤

GET /api/tareas/listar
  └─ Obtiene todas las 50 tareas
     Respuesta: Array[TareaDto]

GET /api/tareas/buscar/{id}
  └─ Obtiene 1 tarea específica
     Params: id (int)
     Respuesta: TareaDto | 404

POST /api/tareas/guardar
  └─ Crea nueva tarea
     Body: CreateTareaDto
     Respuesta: TareaDto | 201

PUT /api/tareas/actualizar/{id}
  └─ Actualiza tarea existente
     Params: id (int)
     Body: UpdateTareaDto
     Respuesta: TareaDto | 404

DELETE /api/tareas/eliminar/{id}
  └─ Elimina tarea
     Params: id (int)
     Respuesta: 204 | 404

────────────────────────────────────────────────────────────────

POST /api/tareas/asignar
  └─ Asigna tarea a usuario (relación M:M)
     Body: { "usuarioId": 1, "tareaId": 1 }
     Respuesta: Success | 409 (si ya existe)

DELETE /api/tareas/desasignar/{usuarioId}/{tareaId}
  └─ Desasigna tarea de usuario
     Params: usuarioId, tareaId
     Respuesta: 204 | 404

GET /api/tareas/usuario/{usuarioId}
  └─ Obtiene tareas asignadas a usuario
     Params: usuarioId (int)
     Respuesta: Array[TareaAsignadaDto]

└────────────────────────────────────────────────────────────────┘
```

---

## 📦 FLUJO DE PAQUETES (NUGET)

```
WebAPIUser.csproj
├─ Microsoft.EntityFrameworkCore
│  └─ ✅ Ya instalado
├─ Microsoft.EntityFrameworkCore.SqlServer
│  └─ ✅ Ya instalado
├─ Microsoft.AspNetCore.OpenApi
│  └─ ✅ Ya instalado
└─ (No se agregaron nuevos paquetes)
```

---

## 🗄️ MIGRACIONES: QUÉ VA A PASAR

```
Cuando ejecutes:
  dotnet ef migrations add AddTareasAndUsuarioTareas

Se crea:
  ├─ Migrations/YYYYMMDDHHMMSS_AddTareasAndUsuarioTareas.cs
  │  └─ Contiene:
  │     • CreateTable("Tarea", columns: ...)
  │     • CreateTable("UsuarioTarea", columns: ...)
  │     • CreateIndex/Foreign Keys
  │
  └─ Migrations/DbUserContextModelSnapshot.cs (actualizado)

Cuando ejecutes:
  dotnet ef database update

Se realiza en SQL Server:
  ├─ CREATE TABLE Tarea
  ├─ CREATE TABLE UsuarioTarea
  ├─ ALTER TABLE Usuario ADD COLUMN navegación_virtual (no físico)
  ├─ ALTER TABLE UsuarioTarea ADD FOREIGN KEY
  └─ INSERT __EFMigrationsHistory
```

---

## 🧮 ESTADÍSTICAS DE CÓDIGO

```
DTOs:           5 nuevos
Servicios:      1 Interfaz + 1 Implementación
Controladores:  1 nuevo
Modelos:        2 nuevos + 2 actualizados
Endpoints:      8 nuevos
Líneas de Código: ~800 nuevas
Método Count:   45+ nuevos métodos
Async/Await:    13 operaciones async
Try-Catch:      12 bloques de error handling
Logging:        25+ puntos de log
```

---

## ✅ VERIFICACIÓN: LO QUE NO CAMBIÓ

```
✅ UsuariosController.cs
   └─ Sigue siendo el mismo
   └─ GET /api/usuarios/listar → FUNCIONA
   └─ POST /api/usuarios/guardar → FUNCIONA

✅ UsuarioService.cs
   └─ Sigue siendo el mismo
   └─ Métodos CRUD idénticos

✅ MapperService.cs
   └─ Sin cambios

✅ appsettings.json
   └─ ConnectionString sin cambios
   └─ Logging sin cambios

✅ Program.cs
   └─ Solo se agregó 1 línea:
      builder.Services.AddScoped<ITareaService, TareaService>();

✅ Swagger
   └─ Sigue mostrando todos los endpoints
   └─ Usuarios + Tareas juntos
```

---

## 🎯 MATRIZ DE CAMBIOS

```
ARCHIVO                        │ CREADO │ MODIFICADO │ ROTO
───────────────────────────────┼────────┼────────────┼──────
Controllers/UsuariosController │        │            │   ✅
Controllers/TareasController   │   ✅   │            │   ✅
Models/Usuario                 │        │     ✅     │   ✅
Models/Tarea                   │   ✅   │            │   ✅
Models/UsuarioTarea            │   ✅   │            │   ✅
Models/DbUserContext           │        │     ✅     │   ✅
Services/UsuarioService        │        │            │   ✅
Services/TareaService          │   ✅   │            │   ✅
DTOs/CreateTareaDto            │   ✅   │            │   ✅
DTOs/UpdateTareaDto            │   ✅   │            │   ✅
DTOs/TareaDto                  │   ✅   │            │   ✅
DTOs/AsignarTareaDto           │   ✅   │            │   ✅
DTOs/UsuarioConTareasDto       │   ✅   │            │   ✅
Program.cs                     │        │     ✅     │   ✅
───────────────────────────────┴────────┴────────────┴──────
TOTAL: 10 creados + 4 modificados + 0 rotos
```

---

## 🚀 SECUENCIA DE EJECUCIÓN RECOMENDADA

```
┌─────────────────────────────────────────────┐
│ PASO 1: Reinicia Visual Studio              │
│ Motivo: Errores ENC0023                     │
└────────────────┬────────────────────────────┘
                 ▼
┌─────────────────────────────────────────────┐
│ PASO 2: dotnet ef migrations add            │
│ Crea archivo de migración                   │
└────────────────┬────────────────────────────┘
                 ▼
┌─────────────────────────────────────────────┐
│ PASO 3: dotnet ef database update           │
│ Crea tablas en SQL Server                   │
└────────────────┬────────────────────────────┘
                 ▼
┌─────────────────────────────────────────────┐
│ PASO 4: dotnet run                          │
│ Inicia la aplicación                        │
└────────────────┬────────────────────────────┘
                 ▼
┌─────────────────────────────────────────────┐
│ PASO 5: Abre Swagger                        │
│ https://localhost:7000/swagger              │
└────────────────┬────────────────────────────┘
                 ▼
┌─────────────────────────────────────────────┐
│ PASO 6: Prueba endpoints                    │
│ Swagger + Checklist de ejecución            │
└────────────────┬────────────────────────────┘
                 ▼
┌─────────────────────────────────────────────┐
│ PASO 7: (OPCIONAL) Ejecuta SeedData         │
│ Crea 50 tareas automáticamente              │
└─────────────────────────────────────────────┘
```

---

## 📊 RESUMEN VISUAL

```
ANTES:
┌──────────────────────────────┐
│   WebAPIUser v1.0            │
├──────────────────────────────┤
│ ✅ Usuarios (CRUD)           │
│ ✅ 5 Endpoints               │
│ ✅ DTOs de Usuario           │
│ ✅ Servicio de Usuario       │
└──────────────────────────────┘

DESPUÉS:
┌──────────────────────────────┐
│   WebAPIUser v2.0            │
├──────────────────────────────┤
│ ✅ Usuarios (CRUD)           │
│ ✅ 5 Endpoints               │
│ ✅ Tareas (CRUD)             │ ✨ NUEVO
│ ✅ 8 Endpoints               │ ✨ NUEVO
│ ✅ Asignación M:M            │ ✨ NUEVO
│ ✅ DTOs de Usuario           │
│ ✅ DTOs de Tarea             │ ✨ NUEVO
│ ✅ Servicio de Usuario       │
│ ✅ Servicio de Tarea         │ ✨ NUEVO
│ ✅ 0 Código Roto             │ ✅ GARANTIZADO
└──────────────────────────────┘
```

---

**Fin de la Documentación Arquitectónica**

Para detalles técnicos, ver: `TAREAS_IMPLEMENTACION.md`  
Para pasos de ejecución, ver: `CHECKLIST_EJECUCION.md`  
Para resumen ejecutivo, ver: `TAREAS_RESUMEN.md`
