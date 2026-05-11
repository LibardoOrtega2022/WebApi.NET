using Microsoft.AspNetCore.Mvc;
using WebAPIUser.DTOs;
using WebAPIUser.Services;

namespace WebAPIUser.Controllers;

/// <summary>
/// Controlador para la gestión de tareas.
/// <br/>Controller for task management.
/// </summary>
[ApiController]
[Route("api/Tasks")]
[ApiExplorerSettings(GroupName = "Tasks")]
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
    /// Obtiene todas las tareas del sistema.
    /// <br/>/Retrieves all tasks from the system.
    /// </summary>
    /// <remarks>
    /// Recupera la lista completa de tareas existentes.
    /// <br/>Retrieves the full list of existing tasks.
    ///
    /// Respuesta exitosa (200): retorna un objeto con lista de tareas, mensaje y timestamp.
    /// <br/>Successful response (200): returns an object containing a list of tasks, a message and a timestamp.
    ///
    /// Posibles errores / Possible errors:
    /// - 400: Error en la base de datos o excepción no manejada / Database error or unhandled exception.
    /// </remarks>
    /// <returns>
    /// Lista completa de tareas con sus propiedades: Id, Titulo, Descripcion, Prioridad, Categoria, FechaVencimiento, FechaCreacion.
    /// <br/>Complete list of tasks with their properties: Id, Title, Description, Priority, Category, DueDate, CreationDate.
    /// </returns>
    /// <response code="200">
    /// Retorna la lista de tareas con mensaje y timestamp.
    /// <br/>Returns the task list with message and timestamp.
    /// </response>
    /// <response code="400">
    /// Error en la base de datos o excepción no manejada.
    /// <br/>Database error or unhandled exception.
    /// </response>
    [HttpGet("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    /// Obtiene una tarea específica por su identificador.
    /// <br/>/Retrieves a specific task by its identifier.
    /// </summary>
    /// <remarks>
    /// Busca y retorna una tarea individual según su ID.
    /// <br/>Searches and returns a single task by its ID.
    ///
    /// Posibles errores / Possible errors:
    /// - 404: Tarea no encontrada con el ID proporcionado / Task not found with the provided ID.
    /// - 400: Error en la base de datos o excepción no manejada / Database error or unhandled exception.
    /// </remarks>
    /// <param name="id">
    /// ID único de la tarea a buscar (debe ser mayor a 0).
    /// <br/>Unique ID of the task to find (must be greater than 0).
    /// </param>
    /// <returns>
    /// Objeto Tarea encontrado con sus propiedades completas.
    /// <br/>Found Task object with all its properties.
    /// </returns>
    /// <response code="200">
    /// Retorna la tarea solicitada con todos sus datos.
    /// <br/>Returns the requested task with all its data.
    /// </response>
    /// <response code="404">
    /// Tarea no encontrada con el ID proporcionado.
    /// <br/>Task not found with the provided ID.
    /// </response>
    /// <response code="400">
    /// Error en la base de datos o excepción no manejada.
    /// <br/>Database error or unhandled exception.
    /// </response>
    [HttpGet("find/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    /// Crea una nueva tarea en el sistema.
    /// <br/>/Creates a new task in the system.
    /// </summary>
    /// <remarks>
    /// Inserta una tarea nueva en la base de datos con validaciones.
    /// <br/>Inserts a new task into the database with validations.
    ///
    /// Validaciones requeridas / Required validations:
    /// - Titulo / Title: requerido, máximo 200 caracteres / required, max 200 characters.
    /// - Descripcion / Description: opcional / optional.
    /// - Prioridad / Priority: valor válido del enum (Baja=1, Media=2, Alta=3, Crítica=4) / valid enum value (Low=1, Medium=2, High=3, Critical=4).
    /// - Categoria / Category: opcional / optional.
    ///
    /// Posibles errores / Possible errors:
    /// - 400: Datos inválidos, título vacío o excede longitud máxima / Invalid data, empty title or exceeds max length.
    /// - 400: Validaciones de modelo no cumplidas / Model validations not met.
    /// - 400: Error en la base de datos / Database error.
    /// </remarks>
    /// <param name="createDto">
    /// DTO con los datos de la nueva tarea: Titulo (requerido), Descripcion, Prioridad (1-4), Categoria, FechaVencimiento.
    /// <br/>DTO with the new task data: Title (required), Description, Priority (1-4), Category, DueDate.
    /// </param>
    /// <returns>
    /// Objeto Tarea creado con ID autogenerado y todos sus datos.
    /// <br/>Created Task object with auto-generated ID and all its data.
    /// </returns>
    /// <response code="201">
    /// Tarea creada exitosamente. Incluye la ubicación del recurso en el header Location.
    /// <br/>Task created successfully. Includes the resource location in the Location header.
    /// </response>
    /// <response code="400">
    /// Datos inválidos o error en la base de datos.
    /// <br/>Invalid data or database error.
    /// </response>
    [HttpPost("save")]
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
    /// Actualiza una tarea existente de forma completa (PUT).
    /// <br/>/Fully updates an existing task (PUT).
    /// </summary>
    /// <remarks>
    /// Modifica todos los datos de una tarea existente.
    /// <br/>Modifies all data of an existing task.
    ///
    /// Validaciones / Validations:
    /// - El ID debe existir en la base de datos / The ID must exist in the database.
    /// - Titulo / Title: requerido, máximo 200 caracteres / required, max 200 characters.
    /// - Prioridad / Priority: valor válido del enum / valid enum value.
    ///
    /// Posibles errores / Possible errors:
    /// - 404: No existe tarea con el ID proporcionado / No task found with the provided ID.
    /// - 400: Datos inválidos o validaciones de modelo fallidas / Invalid data or model validation failed.
    /// - 400: Error en la base de datos o excepción no manejada / Database error or unhandled exception.
    /// </remarks>
    /// <param name="id">
    /// ID de la tarea a actualizar (debe ser mayor a 0 y existir en el sistema).
    /// <br/>ID of the task to update (must be greater than 0 and exist in the system).
    /// </param>
    /// <param name="updateDto">
    /// DTO con los datos actualizados: Titulo, Descripcion, Prioridad (1-4), Categoria, FechaVencimiento.
    /// <br/>DTO with updated data: Title, Description, Priority (1-4), Category, DueDate.
    /// </param>
    /// <returns>
    /// Objeto Tarea actualizado con todos sus campos modificados.
    /// <br/>Updated Task object with all modified fields.
    /// </returns>
    /// <response code="200">
    /// Tarea actualizada exitosamente.
    /// <br/>Task updated successfully.
    /// </response>
    /// <response code="400">
    /// Datos inválidos o error en la base de datos.
    /// <br/>Invalid data or database error.
    /// </response>
    /// <response code="404">
    /// Tarea no encontrada con el ID proporcionado.
    /// <br/>Task not found with the provided ID.
    /// </response>
    [HttpPut("update/{id}")]
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
    /// Elimina una tarea del sistema de forma permanente.
    /// <br/>/Permanently deletes a task from the system.
    /// </summary>
    /// <remarks>
    /// Borra una tarea existente y todas sus asignaciones asociadas (eliminación en cascada).
    /// <br/>Deletes an existing task and all its associated assignments (cascade delete).
    ///
    /// Comportamiento en cascada / Cascade behavior:
    /// - Elimina la tarea de la tabla Tarea / Deletes the task from the Tarea table.
    /// - Elimina automáticamente todas las relaciones M:M en UsuarioTarea / Automatically removes all M:M relationships in UsuarioTarea.
    ///
    /// Posibles errores / Possible errors:
    /// - 404: No existe tarea con el ID proporcionado / No task found with the provided ID.
    /// - 400: Error en la base de datos o excepción no manejada / Database error or unhandled exception.
    /// </remarks>
    /// <param name="id">
    /// ID de la tarea a eliminar (debe ser mayor a 0 y existir en el sistema).
    /// <br/>ID of the task to delete (must be greater than 0 and exist in the system).
    /// </param>
    /// <returns>
    /// 204 Sin contenido si la eliminación fue exitosa. La tarea y todas sus asignaciones se eliminan permanentemente.
    /// <br/>204 No Content if the deletion was successful. The task and all its assignments are permanently deleted.
    /// </returns>
    /// <response code="204">
    /// Tarea eliminada exitosamente. No retorna contenido.
    /// <br/>Task deleted successfully. No content returned.
    /// </response>
    /// <response code="404">
    /// Tarea no encontrada con el ID proporcionado.
    /// <br/>Task not found with the provided ID.
    /// </response>
    /// <response code="400">
    /// Error en la base de datos o excepción no manejada.
    /// <br/>Database error or unhandled exception.
    /// </response>
    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    /// Asigna una tarea a un usuario específico (relación M:M).
    /// <br/>/Assigns a task to a specific user (M:M relationship).
    /// </summary>
    /// <remarks>
    /// Crea una relación entre un usuario y una tarea en la tabla pivote UsuarioTarea.
    /// <br/>Creates a relationship between a user and a task in the UsuarioTarea pivot table.
    ///
    /// Validaciones / Validations:
    /// - El usuario debe existir / The user must exist.
    /// - La tarea debe existir / The task must exist.
    /// - No se permite duplicar la asignación (misma tarea al mismo usuario) / Duplicate assignments are not allowed (same task to the same user).
    ///
    /// Comportamiento / Behavior:
    /// - Se registra FechaAsignacion automáticamente / FechaAsignacion is recorded automatically.
    /// - Estado inicial: Pendiente / Initial status: Pending.
    ///
    /// Posibles errores / Possible errors:
    /// - 404: Usuario o Tarea no existe / User or Task does not exist.
    /// - 409: La tarea ya está asignada a este usuario / Task is already assigned to this user.
    /// - 400: Error en la base de datos o excepción no manejada / Database error or unhandled exception.
    /// </remarks>
    /// <param name="asignarDto">
    /// DTO con los IDs: UsuarioId (ID del usuario receptor) y TareaId (ID de la tarea a asignar).
    /// <br/>DTO with IDs: UsuarioId (receiving user's ID) and TareaId (ID of the task to assign).
    /// </param>
    /// <returns>
    /// Confirmación exitosa con mensaje y timestamp.
    /// <br/>Successful confirmation with message and timestamp.
    /// </returns>
    /// <response code="200">
    /// Tarea asignada exitosamente.
    /// <br/>Task assigned successfully.
    /// </response>
    /// <response code="404">
    /// Usuario o tarea no encontrados.
    /// <br/>User or task not found.
    /// </response>
    /// <response code="409">
    /// La tarea ya está asignada a este usuario.
    /// <br/>The task is already assigned to this user.
    /// </response>
    /// <response code="400">
    /// Error en la base de datos o excepción no manejada.
    /// <br/>Database error or unhandled exception.
    /// </response>
    [HttpPost("assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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
    /// Desasigna una tarea de un usuario (elimina la relación M:M).
    /// <br/>/Unassigns a task from a user (removes the M:M relationship).
    /// </summary>
    /// <remarks>
    /// Elimina la relación entre un usuario y una tarea en la tabla UsuarioTarea.
    /// <br/>Removes the relationship between a user and a task in the UsuarioTarea table.
    ///
    /// Comportamiento / Behavior:
    /// - No afecta la tarea (sigue existiendo) / Does not affect the task (it still exists).
    /// - No afecta al usuario (sigue existiendo) / Does not affect the user (they still exist).
    /// - Solo elimina el registro en la tabla pivote UsuarioTarea / Only removes the record in the UsuarioTarea pivot table.
    ///
    /// Posibles errores / Possible errors:
    /// - 404: La asignación (usuario-tarea) no existe / The assignment (user-task) does not exist.
    /// - 400: Error en la base de datos o excepción no manejada / Database error or unhandled exception.
    /// </remarks>
    /// <param name="usuarioId">
    /// ID del usuario del cual remover la tarea (debe ser mayor a 0).
    /// <br/>ID of the user from whom to remove the task (must be greater than 0).
    /// </param>
    /// <param name="tareaId">
    /// ID de la tarea a remover (debe ser mayor a 0).
    /// <br/>ID of the task to remove (must be greater than 0).
    /// </param>
    /// <returns>
    /// 204 Sin contenido si la desasignación fue exitosa. La relación usuario-tarea se elimina pero ambos registros persisten.
    /// <br/>204 No Content if the unassignment was successful. The user-task relationship is removed but both records persist.
    /// </returns>
    /// <response code="204">
    /// Tarea desasignada exitosamente. No retorna contenido.
    /// <br/>Task unassigned successfully. No content returned.
    /// </response>
    /// <response code="404">
    /// Asignación usuario-tarea no encontrada.
    /// <br/>User-task assignment not found.
    /// </response>
    /// <response code="400">
    /// Error en la base de datos o excepción no manejada.
    /// <br/>Database error or unhandled exception.
    /// </response>
    [HttpDelete("unassign/{usuarioId}/{tareaId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    /// Obtiene todas las tareas asignadas a un usuario específico.
    /// <br/>/Retrieves all tasks assigned to a specific user.
    /// </summary>
    /// <remarks>
    /// Recupera la lista de tareas asociadas a un usuario mediante la tabla pivote UsuarioTarea.
    /// <br/>Retrieves the list of tasks associated with a user through the UsuarioTarea pivot table.
    ///
    /// Comportamiento / Behavior:
    /// - Retorna solo las tareas asignadas al usuario; si no tiene ninguna, retorna lista vacía / Returns only tasks assigned to the user; if none, returns an empty list.
    /// - Cada tarea incluye su estado de asignación (Pendiente, En Progreso, Completada, Cancelada) / Each task includes its assignment status (Pending, In Progress, Completed, Cancelled).
    /// - Incluye FechaAsignacion y FechaCompletado (si aplica) / Includes AssignmentDate and CompletionDate (if applicable).
    ///
    /// Posibles errores / Possible errors:
    /// - 400: Error en la base de datos o excepción no manejada / Database error or unhandled exception.
    /// - Nota / Note: Si el usuario no existe se retorna lista vacía, no un error / If the user does not exist, an empty list is returned, not an error.
    /// </remarks>
    /// <param name="usuarioId">
    /// ID del usuario para obtener sus tareas (debe ser mayor a 0).
    /// <br/>ID of the user to retrieve their tasks (must be greater than 0).
    /// </param>
    /// <returns>
    /// Lista de tareas asignadas al usuario con sus estados de asignación. Puede ser vacía si el usuario no tiene tareas.
    /// <br/>List of tasks assigned to the user with their assignment statuses. Can be empty if the user has no tasks.
    /// </returns>
    /// <response code="200">
    /// Retorna la lista de tareas del usuario (puede ser vacía).
    /// <br/>Returns the user's task list (can be empty).
    /// </response>
    /// <response code="400">
    /// Error en la base de datos o excepción no manejada.
    /// <br/>Database error or unhandled exception.
    /// </response>
    [HttpGet("user/{usuarioId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
