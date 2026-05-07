using WebAPIUser.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace WebAPIUser.Data;

/// <summary>
/// Generador de datos de prueba para Tareas
/// Crea 50 tareas aleatorias con datos realistas
/// </summary>
public static class TareaSeeder
{
    private static readonly Random _random = new();

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
    /// Genera 50 tareas aleatorias
    /// </summary>
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
    /// Genera una tarea individual aleatoria
    /// </summary>
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
    /// Genera un título aleatorio combinando palabras
    /// </summary>
    private static string GenerarTituloAleatorio(int numero)
    {
        var verbo = _titulosBase[_random.Next(_titulosBase.Length)];
        var caracteristica = _caracteristicas[_random.Next(_caracteristicas.Length)];

        return $"{verbo} {caracteristica} #{numero:D2}";
    }

    /// <summary>
    /// Genera una descripción aleatoria
    /// </summary>
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
    private static DateTime GenerarFechaVencimiento()
    {
        var diasFuturos = _random.Next(1, 90); // 1 a 90 días en el futuro
        return DateTime.UtcNow.AddDays(diasFuturos);
    }

    /// <summary>
    /// Inicializa la base de datos con tareas de prueba
    /// </summary>
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
