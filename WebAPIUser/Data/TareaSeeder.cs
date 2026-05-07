using WebAPIUser.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace WebAPIUser.Data;

/// <summary>
/// Generador de datos de prueba para la entidad <see cref="Tarea"/>.
/// Crea 50 tareas con títulos, descripciones, prioridades y categorías aleatorias
/// combinando palabras de pools predefinidos para producir datos realistas.
/// Solo inserta si la tabla <c>Tarea</c> está vacía (idempotente).
/// </summary>
public static class TareaSeeder
{
    /// <summary>Instancia compartida de Random para todas las generaciones aleatorias.</summary>
    private static readonly Random _random = new();

    /// <summary>
    /// Pool de verbos de acción usados como prefijo en los títulos de tareas.
    /// Se combinan con <see cref="_caracteristicas"/> para generar títulos descriptivos.
    /// </summary>
    private static readonly string[] _titulosBase = new[]
    {
        "Implementar",
        "Refactorizar",
        "Corregir",
        "Optimizar",
        "Documentar",
        "Revisar",
        "Probar",
        "Desplegar",
        "Actualizar",
        "Migrar",
        "Integrar",
        "Configurar",
        "Diseñar",
        "Validar",
        "Monitorear"
    };

    /// <summary>
    /// Pool de componentes o áreas técnicas usados como sufijo en los títulos de tareas.
    /// Se combinan con <see cref="_titulosBase"/> para generar títulos descriptivos.
    /// </summary>
    private static readonly string[] _caracteristicas = new[]
    {
        "sistema de autenticación",
        "módulo de reportes",
        "API REST",
        "base de datos",
        "interfaz de usuario",
        "caché de datos",
        "servicio de email",
        "validaciones",
        "seguridad",
        "rendimiento",
        "logging",
        "manejo de errores",
        "tests unitarios",
        "documentación",
        "migraciones",
        "índices de BD",
        "vistas",
        "triggers",
        "procedimientos almacenados",
        "webhooks"
    };

    /// <summary>
    /// Pool de categorías técnicas asignadas aleatoriamente a cada tarea.
    /// Representa el área de trabajo a la que pertenece la tarea.
    /// </summary>
    private static readonly string[] _categorias = new[]
    {
        "Desarrollo",
        "Testing",
        "DevOps",
        "Documentación",
        "Infraestructura",
        "Seguridad",
        "Performance",
        "UI/UX",
        "Backend",
        "Frontend"
    };

    /// <summary>
    /// Pool de frases base para las descripciones de tareas.
    /// Se complementan con un detalle adicional aleatorio de <see cref="GenerarDescripcion"/>.
    /// </summary>
    private static readonly string[] _descripcionesCortas = new[]
    {
        "Completar funcionalidad principal",
        "Resolver bug crítico",
        "Mejorar velocidad de carga",
        "Aumentar cobertura de tests",
        "Actualizar dependencias",
        "Revisar código",
        "Implementar nueva característica",
        "Refactorizar código legacy",
        "Preparar para producción",
        "Validar con stakeholders",
        "Optimizar queries",
        "Mejorar experiencia de usuario",
        "Establecer mejores prácticas",
        "Configurar CI/CD",
        "Documentar API"
    };

    /// <summary>
    /// Genera una lista de tareas aleatorias lista para insertar en la base de datos.
    /// Cada tarea tiene un número secuencial en el título para facilitar su identificación.
    /// </summary>
    /// <param name="cantidad">Número de tareas a generar. Por defecto 50.</param>
    /// <returns>Lista de entidades <see cref="Tarea"/> sin ID (se asigna al persistir).</returns>
    public static List<Tarea> GenerarTareas(int cantidad = 50)
    {
        var tareas = new List<Tarea>();

        for (int i = 1; i <= cantidad; i++)
        {
            tareas.Add(GenerarTareaAleatoria(i));
        }

        return tareas;
    }

    /// <summary>
    /// Genera una única tarea con todos sus campos poblados aleatoriamente.
    /// La prioridad se selecciona de forma uniforme entre los valores del enum.
    /// </summary>
    /// <param name="numero">Número secuencial incluido en el título (ej. #01, #02).</param>
    /// <returns>Entidad <see cref="Tarea"/> lista para persistir.</returns>
    private static Tarea GenerarTareaAleatoria(int numero)
    {
        var prioridades = Enum.GetValues(typeof(PrioridadEnum)).Cast<PrioridadEnum>().ToArray();

        return new Tarea
        {
            Titulo = GenerarTituloAleatorio(numero),
            Descripcion = GenerarDescripcion(),
            Prioridad = prioridades[_random.Next(prioridades.Length)],
            Categoria = _categorias[_random.Next(_categorias.Length)],
            FechaVencimiento = GenerarFechaVencimiento(),
            FechaCreacion = DateTime.UtcNow.AddDays(-_random.Next(30))
        };
    }

    /// <summary>
    /// Construye un título combinando un verbo de <see cref="_titulosBase"/>
    /// con una característica de <see cref="_caracteristicas"/> y el número secuencial.
    /// Ejemplo: "Implementar API REST #07".
    /// </summary>
    /// <param name="numero">Número secuencial para diferenciar tareas con el mismo prefijo.</param>
    /// <returns>Título único de la tarea.</returns>
    private static string GenerarTituloAleatorio(int numero)
    {
        var verbo = _titulosBase[_random.Next(_titulosBase.Length)];
        var caracteristica = _caracteristicas[_random.Next(_caracteristicas.Length)];

        return $"{verbo} {caracteristica} #{numero:D2}";
    }

    /// <summary>
    /// Construye una descripción combinando una frase base de <see cref="_descripcionesCortas"/>
    /// con un detalle adicional aleatorio para dar más contexto a la tarea.
    /// Ejemplo: "Resolver bug crítico antes del deadline."
    /// </summary>
    /// <returns>Descripción completa de la tarea.</returns>
    private static string GenerarDescripcion()
    {
        var descripcion = _descripcionesCortas[_random.Next(_descripcionesCortas.Length)];

        // Agregar detalles adicionales aleatorios
        var detalles = new[]
        {
            " para mejorar la funcionalidad.",
            " según especificaciones.",
            " antes del deadline.",
            " con pruebas incluidas.",
            " y documentar cambios.",
            " en colaboración con el equipo.",
            " siguiendo estándares de código.",
            " para resolver incidencias.",
            " con cobertura de tests.",
            " en ambiente de staging primero."
        };

        return descripcion + detalles[_random.Next(detalles.Length)];
    }

    /// <summary>
    /// Genera una fecha de vencimiento aleatoria entre 1 y 90 días en el futuro.
    /// Siempre retorna un valor para evitar nulos en los datos de prueba.
    /// </summary>
    /// <returns>Fecha futura entre 1 y 90 días desde hoy (UTC).</returns>
    private static DateTime GenerarFechaVencimiento()
    {
        var diasFuturos = _random.Next(1, 90); // 1 a 90 días en el futuro
        return DateTime.UtcNow.AddDays(diasFuturos);
    }

    /// <summary>
    /// Verifica si la tabla <c>Tarea</c> ya tiene registros.
    /// Si está vacía, genera 50 tareas aleatorias y las inserta en bloque.
    /// Imprime en consola el progreso y una muestra de las primeras 5 tareas creadas.
    /// </summary>
    /// <param name="context">Contexto de base de datos activo.</param>
    public static async Task InicializarTareasAsync(DbUserContext context)
    {
        // Verificar si ya existen tareas
        if (await context.Tareas.AsQueryable().AnyAsync())
        {
            Console.WriteLine("✅ Las tareas ya existen en la base de datos");
            return;
        }

        Console.WriteLine("🔄 Generando 50 tareas aleatorias...");

        try
        {
            var tareas = GenerarTareas(50);

            await context.Tareas.AddRangeAsync(tareas);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Se crearon {tareas.Count} tareas exitosamente");

            // Mostrar muestra de tareas creadas
            Console.WriteLine("\n📋 Muestra de tareas creadas:");
            foreach (var tarea in tareas.Take(5))
            {
                Console.WriteLine($"   • {tarea.Titulo} (Prioridad: {tarea.Prioridad}, Categoría: {tarea.Categoria})");
            }
            Console.WriteLine("   ...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al crear tareas: {ex.Message}");
            throw;
        }
    }
}
