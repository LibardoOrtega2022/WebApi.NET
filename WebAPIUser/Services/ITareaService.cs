using WebAPIUser.DTOs;
using WebAPIUser.Models;

namespace WebAPIUser.Services;

/// <summary>
/// Interfaz para operaciones CRUD de tareas
/// </summary>
public interface ITareaService
{
    /// <summary>
    /// Obtiene todas las tareas
    /// </summary>
    Task<IEnumerable<TareaDto>> GetAllTareasAsync();

    /// <summary>
    /// Obtiene una tarea por ID
    /// </summary>
    Task<TareaDto?> GetTareaByIdAsync(int id);

    /// <summary>
    /// Crea una nueva tarea
    /// </summary>
    Task<TareaDto> CreateTareaAsync(CreateTareaDto dto);

    /// <summary>
    /// Actualiza una tarea existente
    /// </summary>
    Task<TareaDto> UpdateTareaAsync(int id, UpdateTareaDto dto);

    /// <summary>
    /// Elimina una tarea
    /// </summary>
    Task<bool> DeleteTareaAsync(int id);

    /// <summary>
    /// Asigna una tarea a un usuario
    /// </summary>
    Task<bool> AsignarTareaAsync(int usuarioId, int tareaId);

    /// <summary>
    /// Desasigna una tarea de un usuario
    /// </summary>
    Task<bool> DesasignarTareaAsync(int usuarioId, int tareaId);

    /// <summary>
    /// Obtiene todas las tareas de un usuario
    /// </summary>
    Task<List<TareaAsignadaDto>> GetTareasPorUsuarioAsync(int usuarioId);
}
