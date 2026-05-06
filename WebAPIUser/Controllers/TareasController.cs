using Microsoft.AspNetCore.Mvc;
using WebAPIUser.DTOs;
using WebAPIUser.Services;

namespace WebAPIUser.Controllers;

/// <summary>
/// Controlador para gestión de tareas
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TareasController : ControllerBase
{
    private readonly ITareaService _tareaService;
    private readonly ILogger<TareasController> _logger;

    public TareasController(
        ITareaService tareaService,
        ILogger<TareasController> logger)
    {
        _tareaService = tareaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las tareas
    /// </summary>
    [HttpGet("listar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetTareas()
    {
        try
        {
            var tareas = await _tareaService.GetAllTareasAsync();
            return Ok(new
            {
                success = true,
                message = "Tareas obtenidas exitosamente",
                data = tareas,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tareas");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene una tarea por ID
    /// </summary>
    [HttpGet("buscar/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetTareaById(int id)
    {
        try
        {
            var tarea = await _tareaService.GetTareaByIdAsync(id);

            if (tarea == null)
                return NotFound(new { success = false, message = $"Tarea con ID {id} no encontrada" });

            return Ok(new
            {
                success = true,
                message = "Tarea obtenida exitosamente",
                data = tarea,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tarea");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Crea una nueva tarea
    /// </summary>
    [HttpPost("guardar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateTarea([FromBody] CreateTareaDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });

            var tarea = await _tareaService.CreateTareaAsync(createDto);
            return CreatedAtAction(nameof(GetTareaById), new { id = tarea.Id }, new
            {
                success = true,
                message = "Tarea creada exitosamente",
                data = tarea,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tarea");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza una tarea existente
    /// </summary>
    [HttpPut("actualizar/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateTarea(int id, [FromBody] UpdateTareaDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });

            var tarea = await _tareaService.UpdateTareaAsync(id, updateDto);
            return Ok(new
            {
                success = true,
                message = "Tarea actualizada exitosamente",
                data = tarea,
                timestamp = DateTime.UtcNow
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Tarea con ID {id} no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tarea");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Elimina una tarea
    /// </summary>
    [HttpDelete("eliminar/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTarea(int id)
    {
        try
        {
            var eliminada = await _tareaService.DeleteTareaAsync(id);

            if (!eliminada)
                return NotFound(new { success = false, message = $"Tarea con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar tarea");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Asigna una tarea a un usuario
    /// </summary>
    [HttpPost("asignar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AsignarTarea([FromBody] AsignarTareaDto asignarDto)
    {
        try
        {
            await _tareaService.AsignarTareaAsync(asignarDto.UsuarioId, asignarDto.TareaId);
            return Ok(new
            {
                success = true,
                message = "Tarea asignada exitosamente",
                timestamp = DateTime.UtcNow
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar tarea");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Desasigna una tarea de un usuario
    /// </summary>
    [HttpDelete("desasignar/{usuarioId}/{tareaId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DesasignarTarea(int usuarioId, int tareaId)
    {
        try
        {
            var desasignada = await _tareaService.DesasignarTareaAsync(usuarioId, tareaId);

            if (!desasignada)
                return NotFound(new { success = false, message = "Asignación no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desasignar tarea");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las tareas asignadas a un usuario
    /// </summary>
    [HttpGet("usuario/{usuarioId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetTareasPorUsuario(int usuarioId)
    {
        try
        {
            var tareas = await _tareaService.GetTareasPorUsuarioAsync(usuarioId);
            return Ok(new
            {
                success = true,
                message = $"Se encontraron {tareas.Count} tareas para el usuario",
                data = tareas,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tareas del usuario");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
