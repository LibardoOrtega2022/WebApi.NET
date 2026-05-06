using WebAPIUser.Models;
using Microsoft.EntityFrameworkCore;

namespace WebAPIUser.Data;

/// <summary>
/// Orquestador principal de seeding de datos
/// Crea usuarios, tareas y asignaciones de forma ordenada
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Inicializa toda la base de datos con datos de prueba
    /// Orden de ejecución:
    /// 1. Crear usuarios (si no existen)
    /// 2. Crear 50 tareas (si no existen)
    /// 3. Asignar tareas a usuarios (si no existen asignaciones)
    /// </summary>
    public static async Task InicializarDatosAsync(DbUserContext context)
    {
        Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          🚀 INICIALIZANDO BASE DE DATOS                  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        try
        {
            // Paso 1: Crear usuarios
            Console.WriteLine("📌 Paso 1: Creando usuarios...");
            await UsuarioSeeder.InicializarUsuariosAsync(context);
            Console.WriteLine();

            // Paso 2: Crear tareas
            Console.WriteLine("📌 Paso 2: Generando tareas...");
            await TareaSeeder.InicializarTareasAsync(context);
            Console.WriteLine();

            // Paso 3: Asignar tareas a usuarios
            Console.WriteLine("📌 Paso 3: Asignando tareas a usuarios...");
            await UsuarioTareaSeeder.AsignarTareasAUsuariosAsync(context);
            Console.WriteLine();

            // Resumen final
            MostrarResumenFinal(context);

            Console.WriteLine("\n✅ ¡INICIALIZACIÓN COMPLETADA EXITOSAMENTE!\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error durante la inicialización: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}\n");
            throw;
        }
    }

    /// <summary>
    /// Muestra un resumen de los datos creados
    /// </summary>
    private static void MostrarResumenFinal(DbUserContext context)
    {
        try
        {
            var usuariosCount = context.Usuarios.Count();
            var tareasCount = context.Tareas.Count();
            var asignacionesCount = context.UsuarioTareas.Count();

            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    📊 RESUMEN FINAL                       ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ 👥 Usuarios:              {usuariosCount,2} usuarios                    ║");
            Console.WriteLine($"║ 📋 Tareas:               {tareasCount,2} tareas                      ║");
            Console.WriteLine($"║ 🔗 Asignaciones M:M:     {asignacionesCount,2} asignaciones              ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");

            if (usuariosCount > 0 && tareasCount > 0)
            {
                var promedioPorUsuario = Math.Round((double)asignacionesCount / usuariosCount, 1);
                Console.WriteLine($"║ ⚖️  Promedio por usuario: {promedioPorUsuario,4} tareas/usuario          ║");
            }

            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        }
        catch
        {
            // Si hay error consultando, simplemente continúa
        }
    }
}
