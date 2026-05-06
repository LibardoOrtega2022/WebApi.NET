# 🤖 SCRIPT DE GENERACIÓN DE DATOS (SEEDING)

## 📋 Descripción General

He creado **4 scripts automáticos** que generan datos de prueba para tu proyecto:

1. **TareaSeeder.cs** - Genera 50 tareas aleatorias
2. **UsuarioSeeder.cs** - Genera 10 usuarios aleatorios  
3. **UsuarioTareaSeeder.cs** - Asigna tareas a usuarios (M:M)
4. **DataSeeder.cs** - Orquestador que ejecuta todo

---

## 🎯 ¿QUÉ HACE?

### Ejecución Automática al Iniciar la App

```
dotnet run
    ↓
Program.cs ejecuta DataSeeder.InicializarDatosAsync()
    ↓
┌─ Paso 1: Crear 10 usuarios (si no existen)
├─ Paso 2: Crear 50 tareas (si no existen)
└─ Paso 3: Asignar tareas a usuarios (si no existen)
    ↓
✅ 10 usuarios + 50 tareas + 50 asignaciones en tu BD
```

---

## 📊 DATOS GENERADOS

### Usuarios (10)
```
Nombres aleatorios: Juan, María, Carlos, Ana, Luis, Patricia, Roberto, Laura, Fernando, Isabel
Apellidos aleatorios: García, Rodríguez, Martínez, Hernández, López, González, Pérez, Sánchez, etc.
Correos únicos: juan.garcia@gmail.com, maria.rodriguez@outlook.com, etc.
Usernames: juangarcia1, mariarodriguez2, etc.
Fecha creación: Hace 0-90 días
```

### Tareas (50)
```
Títulos: "Implementar sistema de autenticación #01", "Refactorizar módulo de reportes #02", etc.
Descripciones: Variadas y realistas ("Completar funcionalidad principal para mejorar...", etc.)
Prioridades: Baja (1), Media (2), Alta (3), Crítica (4) - aleatorias
Categorías: Desarrollo, Testing, DevOps, Documentación, Infraestructura, Seguridad, etc.
Fechas de vencimiento: 1-60 días en el futuro (80% tienen, 20% sin fecha)
Fechas de creación: Hace 0-30 días
```

### Asignaciones M:M (50)
```
Distribución: ~5 tareas por usuario (50 tareas ÷ 10 usuarios)
Estados de asignación:
  - 50% Pendiente
  - 30% En Progreso
  - 15% Completada
  - 5% Cancelada
Algunas tareas completadas tienen fecha de completado
```

---

## 🔧 ESTRUCTURA DE LOS SCRIPTS

### TareaSeeder.cs
```csharp
public static class TareaSeeder
{
    // Datos para generar títulos aleatorios
    private static readonly string[] _titulosBase = ["Implementar", "Refactorizar", ...]
    private static readonly string[] _caracteristicas = ["sistema de autenticación", ...]
    private static readonly string[] _categorias = ["Desarrollo", "Testing", ...]

    // Métodos principales
    public static List<Tarea> GenerarTareas(int cantidad = 50)
    public static async Task InicializarTareasAsync(DbUserContext context)

    // Métodos privados
    private static Tarea GenerarTareaAleatoria(int numero)
    private static string GenerarTituloAleatorio(int numero)
    private static string GenerarDescripcion()
    private static DateTime? GenerarFechaVencimiento()
}
```

### UsuarioSeeder.cs
```csharp
public static class UsuarioSeeder
{
    // Datos para generar usuarios
    private static readonly string[] _nombres = ["Juan", "María", ...]
    private static readonly string[] _apellidos = ["García", "Rodríguez", ...]
    private static readonly string[] _dominios = ["gmail.com", "outlook.com", ...]

    // Métodos principales
    public static List<Usuario> GenerarUsuarios(int cantidad = 10)
    public static async Task InicializarUsuariosAsync(DbUserContext context)

    // Métodos privados
    private static Usuario GenerarUsuarioAleatorio(int numero)
    private static string GenerarCorreoUnico(...)
    private static string GenerarUsername(...)
}
```

### UsuarioTareaSeeder.cs
```csharp
public static class UsuarioTareaSeeder
{
    // Métodos principales
    public static async Task AsignarTareasAUsuariosAsync(DbUserContext context)

    // Métodos privados
    private static List<Tarea> ObtenerTareasAleatorias(...)
    private static EstadoTareaEnum GenerarEstadoAleatorio()
    private static DateTime? GenerarFechaCompletado()
    private static void MostrarEstadisticas(...)
}
```

### DataSeeder.cs
```csharp
public static class DataSeeder
{
    // Orquestador principal
    public static async Task InicializarDatosAsync(DbUserContext context)
    {
        // Paso 1: Crear usuarios
        // Paso 2: Crear tareas
        // Paso 3: Asignar tareas a usuarios
        // Mostrar resumen
    }
}
```

---

## ⚙️ CÓMO FUNCIONA

### Integración en Program.cs

```csharp
// Antes de app.Run()
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbUserContext>();

    // Aplicar migraciones
    await db.Database.MigrateAsync();

    // Ejecutar seeders
    await DataSeeder.InicializarDatosAsync(db);
}

app.Run();
```

### Lógica de Seguridad

✅ Cada seeder **verifica si los datos ya existen**  
✅ Si existen, **no vuelve a crearlos**  
✅ Los correos son **únicos**  
✅ Las asignaciones **no se duplican**  

---

## 🚀 CÓMO EJECUTAR

### Opción 1: Automático (Recomendado)
```bash
# Solo inicia la app
dotnet run

# Los seeders se ejecutan automáticamente al iniciar
# Verás en consola:
# ╔════════════════════════════════════════════════════════════╗
# ║          🚀 INICIALIZANDO BASE DE DATOS                  ║
# ╚════════════════════════════════════════════════════════════╝
# 
# 📌 Paso 1: Creando usuarios...
# ✅ Se crearon 10 usuarios exitosamente
# ...
```

### Opción 2: Manual desde Código
```csharp
// En cualquier controlador o servicio
using WebAPIUser.Data;

public class AlgunControlador : ControllerBase
{
    private readonly DbUserContext _context;

    [HttpPost("seed-data")]
    public async Task<IActionResult> SembrarDatos()
    {
        await DataSeeder.InicializarDatosAsync(_context);
        return Ok("Datos generados exitosamente");
    }
}
```

---

## 📊 EJEMPLO DE SALIDA EN CONSOLA

```
╔════════════════════════════════════════════════════════════╗
║          🚀 INICIALIZANDO BASE DE DATOS                  ║
╚════════════════════════════════════════════════════════════╝

📌 Paso 1: Creando usuarios...
🔄 Generando 10 usuarios aleatorios...
✅ Se crearon 10 usuarios exitosamente

📋 Usuarios creados:
   • Juan García (juan.garcia@gmail.com)
   • María Rodríguez (maria.rodriguez@outlook.com)
   • Carlos Martínez (carlos.martinez@yahoo.com)
   • Ana Hernández (ana.hernandez@gmail.com)
   • Luis López (luis.lopez@empresa.com)
   ...

📌 Paso 2: Generando tareas...
🔄 Generando 50 tareas aleatorias...
✅ Se crearon 50 tareas exitosamente

📋 Muestra de tareas creadas:
   • Implementar sistema de autenticación #01 (Prioridad: Media, Categoría: Desarrollo)
   • Refactorizar módulo de reportes #02 (Prioridad: Baja, Categoría: Testing)
   • Corregir validaciones #03 (Prioridad: Alta, Categoría: Backend)
   • Optimizar queries #04 (Prioridad: Crítica, Categoría: Performance)
   • Desplegar en producción #05 (Prioridad: Media, Categoría: DevOps)
   ...

📌 Paso 3: Asignando tareas a usuarios...
🔄 Asignando 50 tareas a 10 usuarios...
✅ Se asignaron 50 tareas a usuarios exitosamente

📊 Estadísticas de Asignaciones:
═══════════════════════════════════════════════════════
👤 Juan García: 5 tareas
   Estados: Pendiente (3), En Progreso (2)
👤 María Rodríguez: 5 tareas
   Estados: Pendiente (2), Completada (2), En Progreso (1)
👤 Carlos Martínez: 5 tareas
   Estados: Pendiente (4), En Progreso (1)
...
═══════════════════════════════════════════════════════

✅ ¡INICIALIZACIÓN COMPLETADA EXITOSAMENTE!
```

---

## 🔄 CARACTERÍSTICAS PRINCIPALES

### 1️⃣ Datos Realistas
- Nombres y apellidos españoles variados
- Correos con múltiples dominios
- Descripciones detalladas
- Títulos compostos y coherentes

### 2️⃣ Aleatorios pero Distribuidos
- Prioridades distribuidas aleatoriamente
- Estados distribuidos (50% Pendiente, 30% En Progreso, etc.)
- Categorías variadas
- Fechas aleatorias pero coherentes

### 3️⃣ Sin Duplicados
- Correos únicos garantizados
- Usernames únicos
- Asignaciones sin duplicar
- IDs válidos

### 4️⃣ Seguro y Reutilizable
- Verifica datos existentes antes de crear
- Idempotente (ejecutar 2 veces = mismo resultado)
- Manejo de errores completo
- Logging detallado

---

## 🎯 CASOS DE USO

### Caso 1: Desarrollo Local
```bash
# Inicias la app
dotnet run

# Se crean los datos automáticamente
# Ya tienes 10 usuarios + 50 tareas para probar
```

### Caso 2: Tests de Integración
```csharp
[Fact]
public async Task TestObtenerTareas()
{
    // Setup
    var context = new DbUserContext(...);
    await DataSeeder.InicializarDatosAsync(context);

    // Act
    var tareas = await context.Tareas.ToListAsync();

    // Assert
    Assert.Equal(50, tareas.Count);
}
```

### Caso 3: Demostración/Presentación
```bash
# Reinicia la app y tienes datos frescos cada vez
dotnet run

# O desde un endpoint
POST /api/seed-data
```

---

## ⚠️ NOTAS IMPORTANTES

### ✅ Idempotente
- Puedes ejecutar múltiples veces sin problemas
- No crea duplicados
- Verifica datos existentes

### ✅ Respeta tu Estructura
- Usa los enums `PrioridadEnum` y `EstadoTareaEnum`
- Sigue las validaciones del modelo
- Cumple con `[Required]` y `[MaxLength]`

### ✅ Async/Await
- Todos los métodos son asincronos
- Integrable en programas async
- Seguro para concurrencia

### ❌ No Destructivo
- No borra datos existentes
- Solo agrega si no existen
- Seguro ejecutar en producción (aunque no recomendado)

---

## 🔧 PERSONALIZACIÓN

Si quieres modificar los datos generados:

### Cambiar Cantidad de Usuarios
```csharp
// En TareaSeeder.cs, método GenerarUsuarios
public static List<Usuario> GenerarUsuarios(int cantidad = 15)  // ← cambiar
```

### Cambiar Cantidad de Tareas
```csharp
// En TareaSeeder.cs, método GenerarTareas
public static List<Tarea> GenerarTareas(int cantidad = 100)  // ← cambiar
```

### Agregar Más Títulos
```csharp
private static readonly string[] _titulosBase = new[]
{
    "Implementar",
    "Refactorizar",
    "Corregir",
    "TU_NUEVO_TITULO",  // ← agregar
    ...
};
```

---

## ✅ VERIFICACIÓN

Una vez ejecutado, verifica en Swagger:

```
GET /api/usuarios/listar
→ Deberías ver 10 usuarios

GET /api/tareas/listar
→ Deberías ver 50 tareas

GET /api/tareas/usuario/1
→ Deberías ver ~5 tareas asignadas
```

---

## 📁 ARCHIVOS CREADOS

```
WebAPIUser/
└── Data/
    ├── TareaSeeder.cs              ✅ Genera tareas
    ├── UsuarioSeeder.cs            ✅ Genera usuarios
    ├── UsuarioTareaSeeder.cs       ✅ Asigna tareas
    └── DataSeeder.cs               ✅ Orquestador principal
```

---

## 🚀 PRÓXIMOS PASOS

1. **Reinicia Visual Studio** (por los errores ENC0102)
2. **Ejecuta migraciones**: `dotnet ef database update`
3. **Inicia la app**: `dotnet run`
4. **Verifica en Swagger** que los datos se crearon

---

**¡Listo para generar datos! 🎉**
