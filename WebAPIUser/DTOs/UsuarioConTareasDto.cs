using WebAPIUser.Models;

namespace WebAPIUser.DTOs;

/// <summary>
/// DTO para mostrar tareas de un usuario
/// </summary>
public class UsuarioConTareasDto
{
    public int Id { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string? Username { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public List<TareaAsignadaDto> Tareas { get; set; } = [];
}

/// <summary>
/// DTO para tarea asignada a usuario (relación)
/// </summary>
public class TareaAsignadaDto
{
    public int TareaId { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public PrioridadEnum Prioridad { get; set; }

    public EstadoTareaEnum EstadoAsignacion { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public DateTime? FechaCompletado { get; set; }
}
