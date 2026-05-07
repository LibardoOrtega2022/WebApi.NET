using WebAPIUser.Models;
using Microsoft.EntityFrameworkCore;

namespace WebAPIUser.Data;

/// <summary>
/// Orquestador principal del proceso de seeding de datos.
/// Coordina la creación de usuarios, tareas y asignaciones en el orden correcto,
/// garantizando que las dependencias entre tablas se respeten.
/// Es idempotente: si los datos ya existen, no realiza ninguna inserción.
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Punto de entrada del proceso de inicialización de datos.
    /// Ejecuta los tres pasos en orden secuencial:
    /// <list type="number">
    ///   <item>Crear 10 usuarios (si la tabla está vacía).</item>
    ///   <item>Crear 50 tareas aleatorias (si la tabla está vacía).</item>
    ///   <item>Asignar tareas a usuarios en la tabla pivote (si no hay asignaciones).</item>
    /// </list>
    /// Si cualquier paso falla, relanza la excepción para que el error sea visible al iniciar la app.
    /// </summary>
    /// <param name="context">Contexto de base de datos activo.</param>
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
    /// Imprime en consola un resumen con los totales de registros en cada tabla
    /// y el promedio de tareas por usuario.
    /// Se ejecuta al final del proceso de inicialización.
    /// Los errores de consulta se ignoran silenciosamente para no interrumpir el arranque.
    /// </summary>
    /// <param name="context">Contexto de base de datos activo.</param>
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
