using WebAPIUser.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace WebAPIUser.Data;

/// <summary>
/// Generador de asignaciones de tareas a usuarios
/// Asigna 5 tareas por usuario (50 tareas ÷ 10 usuarios)
/// </summary>
public static class UsuarioTareaSeeder
{
    private static readonly Random _random = new();

    /// <summary>
    /// Asigna tareas aleatorias a usuarios existentes
    /// Cada usuario recibe aproximadamente 5 tareas
    /// </summary>
    public static async Task AsignarTareasAUsuariosAsync(DbUserContext context)
    {
        // Obtener usuarios y tareas existentes
        var usuarios = await context.Usuarios.AsQueryable().ToListAsync();
        var tareas = await context.Tareas.AsQueryable().ToListAsync();

        if (usuarios.Count == 0)
        {
            Console.WriteLine("⚠️  No hay usuarios en la base de datos");
            return;
        }

        if (tareas.Count == 0)
        {
            Console.WriteLine("⚠️  No hay tareas en la base de datos");
            return;
        }

        // Verificar si ya existen asignaciones
        if (await context.UsuarioTareas.AsQueryable().AnyAsync())
        {
            Console.WriteLine("✅ Las asignaciones de tareas ya existen");
            return;
        }

        Console.WriteLine($"🔄 Asignando {tareas.Count} tareas a {usuarios.Count} usuarios...");

        try
        {
            var asignaciones = new List<UsuarioTarea>();
            var tareasAsignadas = 0;
            var tareasUsadas = new HashSet<int>();

            // Distribuir tareas entre usuarios
            for (int u = 0; u < usuarios.Count; u++)
            {
                var usuario = usuarios[u];
                var tareasParaUsuario = ObtenerTareasAleatorias(tareas, usuarios.Count, u);

                foreach (var tarea in tareasParaUsuario)
                {
                    // Evitar asignaciones duplicadas
                    if (!tareasUsadas.Contains(tarea.Id))
                    {
                        var estado = GenerarEstadoAleatorio();

                        asignaciones.Add(new UsuarioTarea
                        {
                            UsuarioId = usuario.Id,
                            TareaId = tarea.Id,
                            FechaAsignacion = DateTime.UtcNow.AddDays(-_random.Next(1, 30)),
                            Estado = estado,
                            FechaCompletado = GenerarFechaCompletado(estado)
                        });

                        tareasUsadas.Add(tarea.Id);
                        tareasAsignadas++;
                    }
                }
            }

            await context.UsuarioTareas.AddRangeAsync(asignaciones);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Se asignaron {tareasAsignadas} tareas a usuarios exitosamente");

            // Mostrar estadísticas
            MostrarEstadisticas(usuarios, asignaciones);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al asignar tareas: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Obtiene un conjunto de tareas aleatorias para un usuario
    /// Intenta darle aproximadamente (total_tareas / total_usuarios) tareas
    /// </summary>
    private static List<Tarea> ObtenerTareasAleatorias(List<Tarea> tareas, int cantidadUsuarios, int indiceUsuario)
    {
        var tareasParaUsuario = new List<Tarea>();
        var cantidadTareasPorUsuario = (int)Math.Ceiling((double)tareas.Count / cantidadUsuarios);

        // Agregar variabilidad: algunos usuarios obtienen una tarea más/menos
        if (_random.Next(100) < 30) // 30% de probabilidad
        {
            cantidadTareasPorUsuario += _random.Next(-1, 2);
        }

        // Obtener tareas aleatorias sin repetición
        var tareasDisponibles = tareas.OrderBy(_ => _random.Next()).Take(cantidadTareasPorUsuario).ToList();

        return tareasDisponibles;
    }

    /// <summary>
    /// Genera un estado aleatorio para la asignación
    /// </summary>
    private static EstadoTareaEnum GenerarEstadoAleatorio()
    {
        var estados = Enum.GetValues(typeof(EstadoTareaEnum)).Cast<EstadoTareaEnum>().ToArray();
        var probabilidades = new[] { 50, 30, 15, 5 }; // Probabilidades en porcentaje

        var rand = _random.Next(100);
        int suma = 0;

        for (int i = 0; i < probabilidades.Length; i++)
        {
            suma += probabilidades[i];
            if (rand < suma)
                return estados[i];
        }

        return EstadoTareaEnum.Pendiente;
    }

    /// <summary>
    /// Genera una fecha de completado coherente con el estado de la asignación.
    /// Solo Completada y Cancelada tienen fecha; Pendiente y EnProgreso no.
    /// </summary>
    private static DateTime? GenerarFechaCompletado(EstadoTareaEnum estado)
    {
        return estado switch
        {
            EstadoTareaEnum.Completada => DateTime.UtcNow.AddDays(-_random.Next(1, 30)),
            EstadoTareaEnum.Cancelada  => DateTime.UtcNow.AddDays(-_random.Next(1, 15)),
            _                          => null
        };
    }

    /// <summary>
    /// Muestra estadísticas de las asignaciones
    /// </summary>
    private static void MostrarEstadisticas(List<Usuario> usuarios, List<UsuarioTarea> asignaciones)
    {
        Console.WriteLine("\n📊 Estadísticas de Asignaciones:");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        var estadosPorUsuario = asignaciones.GroupBy(a => a.UsuarioId);

        foreach (var grupo in estadosPorUsuario)
        {
            var usuario = usuarios.FirstOrDefault(u => u.Id == grupo.Key);
            if (usuario != null)
            {
                var estados = grupo.GroupBy(a => a.Estado)
                    .Select(g => $"{g.Key} ({g.Count()})")
                    .ToList();

                Console.WriteLine($"👤 {usuario.Nombres} {usuario.Apellidos}: {grupo.Count()} tareas");
                Console.WriteLine($"   Estados: {string.Join(", ", estados)}");
            }
        }

        Console.WriteLine("═══════════════════════════════════════════════════════\n");
    }
}
