using WebAPIUser.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace WebAPIUser.Data;

/// <summary>
/// Generador de asignaciones de tareas a usuarios para la tabla pivote <c>UsuarioTarea</c>.
/// Distribuye las tareas existentes entre los usuarios existentes de forma aleatoria,
/// asignando aproximadamente (total_tareas / total_usuarios) tareas por usuario.
/// Garantiza que no se creen asignaciones duplicadas (mismo usuario + misma tarea).
/// Solo inserta si la tabla <c>UsuarioTarea</c> está vacía (idempotente).
/// </summary>
public static class UsuarioTareaSeeder
{
    /// <summary>Instancia compartida de Random para todas las generaciones aleatorias.</summary>
    private static readonly Random _random = new();

    /// <summary>
    /// Punto de entrada del seeder de asignaciones.
    /// Carga usuarios y tareas existentes, verifica que ambas tablas tengan datos,
    /// y distribuye las tareas entre los usuarios evitando duplicados.
    /// Imprime estadísticas por usuario al finalizar.
    /// </summary>
    /// <param name="context">Contexto de base de datos activo.</param>
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
    /// Selecciona aleatoriamente un subconjunto de tareas para asignar a un usuario.
    /// La cantidad base es <c>ceil(totalTareas / totalUsuarios)</c>.
    /// Con un 30% de probabilidad, se ajusta en ±1 para agregar variabilidad natural.
    /// Las tareas se ordenan aleatoriamente antes de tomar el subconjunto.
    /// </summary>
    /// <param name="tareas">Lista completa de tareas disponibles.</param>
    /// <param name="cantidadUsuarios">Total de usuarios entre los que se distribuyen las tareas.</param>
    /// <param name="indiceUsuario">Índice del usuario actual (no usado en la selección, reservado para extensiones).</param>
    /// <returns>Subconjunto aleatorio de tareas para el usuario.</returns>
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
    /// Selecciona un estado de asignación con distribución de probabilidad ponderada:
    /// <list type="bullet">
    ///   <item>50% → Pendiente (estado más común al inicio)</item>
    ///   <item>30% → EnProgreso</item>
    ///   <item>15% → Completada</item>
    ///   <item>5%  → Cancelada</item>
    /// </list>
    /// </summary>
    /// <returns>Estado aleatorio ponderado del enum <see cref="EstadoTareaEnum"/>.</returns>
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
    /// <list type="bullet">
    ///   <item><see cref="EstadoTareaEnum.Completada"/> → fecha entre 1 y 30 días atrás.</item>
    ///   <item><see cref="EstadoTareaEnum.Cancelada"/>  → fecha entre 1 y 15 días atrás.</item>
    ///   <item>Cualquier otro estado → <c>null</c> (la tarea no ha terminado).</item>
    /// </list>
    /// </summary>
    /// <param name="estado">Estado de la asignación para determinar si aplica fecha de completado.</param>
    /// <returns>Fecha pasada si el estado es terminal, <c>null</c> en caso contrario.</returns>
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
    /// Imprime en consola un desglose por usuario mostrando cuántas tareas recibió
    /// y cuántas hay en cada estado. Útil para verificar la distribución del seeding.
    /// </summary>
    /// <param name="usuarios">Lista de usuarios para resolver nombres por ID.</param>
    /// <param name="asignaciones">Lista de asignaciones recién creadas.</param>
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
