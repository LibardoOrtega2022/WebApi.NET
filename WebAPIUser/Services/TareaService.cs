using Microsoft.EntityFrameworkCore;
using WebAPIUser.DTOs;
using WebAPIUser.Models;

namespace WebAPIUser.Services;

/// <summary>
/// Servicio para operaciones CRUD de tareas
/// </summary>
public class TareaService : ITareaService
{
    private readonly DbUserContext _context;
    private readonly ILogger<TareaService> _logger;

    public TareaService(
        DbUserContext context,
        ILogger<TareaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TareaDto>> GetAllTareasAsync()
    {
        _logger.LogInformation("Obteniendo todas las tareas");

        var tareas = await _context.Tareas
            .AsNoTracking()
            .ToListAsync();

        var tareasDto = tareas.Select(t => new TareaDto
        {
            Id = t.Id,
            Titulo = t.Titulo,
            Descripcion = t.Descripcion,
            Prioridad = t.Prioridad,
            FechaVencimiento = t.FechaVencimiento,
            Categoria = t.Categoria,
            FechaCreacion = t.FechaCreacion,
            TotalAsignaciones = t.UsuarioTareas.Count
        });

        _logger.LogInformation("Se retornaron {Count} tareas", tareas.Count);
        return tareasDto;
    }

    public async Task<TareaDto?> GetTareaByIdAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("ID de tarea inválido: {Id}", id);
            throw new ArgumentException("El ID debe ser mayor a 0");
        }

        _logger.LogInformation("Buscando tarea con ID: {Id}", id);

        var tarea = await _context.Tareas
            .AsNoTracking()
            .Include(t => t.UsuarioTareas)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tarea == null)
        {
            _logger.LogWarning("Tarea no encontrada con ID: {Id}", id);
            return null;
        }

        var tareaDto = new TareaDto
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Prioridad = tarea.Prioridad,
            FechaVencimiento = tarea.FechaVencimiento,
            Categoria = tarea.Categoria,
            FechaCreacion = tarea.FechaCreacion,
            TotalAsignaciones = tarea.UsuarioTareas.Count
        };

        return tareaDto;
    }

    public async Task<TareaDto> CreateTareaAsync(CreateTareaDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Intento de crear tarea con DTO nulo");
            throw new ArgumentNullException(nameof(dto));
        }

        _logger.LogInformation("Creando nueva tarea: {Titulo}", dto.Titulo);

        var tarea = new Tarea
        {
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            Prioridad = dto.Prioridad,
            FechaVencimiento = dto.FechaVencimiento,
            Categoria = dto.Categoria,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Tareas.Add(tarea);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tarea creada exitosamente con ID: {Id}", tarea.Id);

        return new TareaDto
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Prioridad = tarea.Prioridad,
            FechaVencimiento = tarea.FechaVencimiento,
            Categoria = tarea.Categoria,
            FechaCreacion = tarea.FechaCreacion,
            TotalAsignaciones = 0
        };
    }

    public async Task<TareaDto> UpdateTareaAsync(int id, UpdateTareaDto dto)
    {
        if (id <= 0)
            throw new ArgumentException("El ID debe ser mayor a 0");

        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        _logger.LogInformation("Actualizando tarea con ID: {Id}", id);

        var tarea = await _context.Tareas.FindAsync(id);

        if (tarea == null)
        {
            _logger.LogWarning("Tarea no encontrada: {Id}", id);
            throw new KeyNotFoundException($"Tarea con ID {id} no encontrada");
        }

        tarea.Titulo = dto.Titulo;
        tarea.Descripcion = dto.Descripcion;
        tarea.Prioridad = dto.Prioridad;
        tarea.FechaVencimiento = dto.FechaVencimiento;
        tarea.Categoria = dto.Categoria;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Tarea actualizada: {Id}", id);

        return new TareaDto
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Prioridad = tarea.Prioridad,
            FechaVencimiento = tarea.FechaVencimiento,
            Categoria = tarea.Categoria,
            FechaCreacion = tarea.FechaCreacion,
            TotalAsignaciones = tarea.UsuarioTareas.Count
        };
    }

    public async Task<bool> DeleteTareaAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("El ID debe ser mayor a 0");

        _logger.LogInformation("Eliminando tarea con ID: {Id}", id);

        var tarea = await _context.Tareas.FindAsync(id);

        if (tarea == null)
        {
            _logger.LogWarning("Tarea no encontrada para eliminar: {Id}", id);
            return false;
        }

        _context.Tareas.Remove(tarea);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tarea eliminada: {Id}", id);
        return true;
    }

    public async Task<bool> AsignarTareaAsync(int usuarioId, int tareaId)
    {
        if (usuarioId <= 0 || tareaId <= 0)
        {
            _logger.LogWarning("IDs inválidos - UsuarioId: {UsuarioId}, TareaId: {TareaId}", usuarioId, tareaId);
            throw new ArgumentException("Los IDs deben ser mayores a 0");
        }

        _logger.LogInformation("Asignando tarea {TareaId} a usuario {UsuarioId}", tareaId, usuarioId);

        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        var tarea = await _context.Tareas.FindAsync(tareaId);

        if (usuario == null)
        {
            _logger.LogWarning("Usuario no encontrado: {UsuarioId}", usuarioId);
            throw new KeyNotFoundException($"Usuario con ID {usuarioId} no encontrado");
        }

        if (tarea == null)
        {
            _logger.LogWarning("Tarea no encontrada: {TareaId}", tareaId);
            throw new KeyNotFoundException($"Tarea con ID {tareaId} no encontrada");
        }

        // Verificar si ya existe la asignación
        var existeAsignacion = await _context.UsuarioTareas
            .AnyAsync(ut => ut.UsuarioId == usuarioId && ut.TareaId == tareaId);

        if (existeAsignacion)
        {
            _logger.LogWarning("La tarea ya está asignada al usuario");
            throw new InvalidOperationException("La tarea ya está asignada a este usuario");
        }

        var usuarioTarea = new UsuarioTarea
        {
            UsuarioId = usuarioId,
            TareaId = tareaId,
            FechaAsignacion = DateTime.UtcNow,
            Estado = EstadoTareaEnum.Pendiente
        };

        _context.UsuarioTareas.Add(usuarioTarea);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tarea asignada exitosamente");
        return true;
    }

    public async Task<bool> DesasignarTareaAsync(int usuarioId, int tareaId)
    {
        if (usuarioId <= 0 || tareaId <= 0)
            throw new ArgumentException("Los IDs deben ser mayores a 0");

        _logger.LogInformation("Desasignando tarea {TareaId} de usuario {UsuarioId}", tareaId, usuarioId);

        var asignacion = await _context.UsuarioTareas
            .FirstOrDefaultAsync(ut => ut.UsuarioId == usuarioId && ut.TareaId == tareaId);

        if (asignacion == null)
        {
            _logger.LogWarning("Asignación no encontrada");
            return false;
        }

        _context.UsuarioTareas.Remove(asignacion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tarea desasignada exitosamente");
        return true;
    }

    public async Task<List<TareaAsignadaDto>> GetTareasPorUsuarioAsync(int usuarioId)
    {
        if (usuarioId <= 0)
            throw new ArgumentException("El ID debe ser mayor a 0");

        _logger.LogInformation("Obteniendo tareas del usuario: {UsuarioId}", usuarioId);

        var tareas = await _context.UsuarioTareas
            .AsNoTracking()
            .Where(ut => ut.UsuarioId == usuarioId)
            .Include(ut => ut.Tarea)
            .Select(ut => new TareaAsignadaDto
            {
                TareaId = ut.Tarea.Id,
                Titulo = ut.Tarea.Titulo,
                Descripcion = ut.Tarea.Descripcion,
                Prioridad = ut.Tarea.Prioridad,
                EstadoAsignacion = ut.Estado,
                FechaAsignacion = ut.FechaAsignacion,
                FechaCompletado = ut.FechaCompletado
            })
            .ToListAsync();

        _logger.LogInformation("Se retornaron {Count} tareas para usuario {UsuarioId}", tareas.Count, usuarioId);
        return tareas;
    }
}
