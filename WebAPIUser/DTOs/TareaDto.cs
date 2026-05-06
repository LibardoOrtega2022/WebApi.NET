using WebAPIUser.Models;

namespace WebAPIUser.DTOs;

/// <summary>
/// DTO para respuestas de tarea
/// </summary>
public class TareaDto
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public PrioridadEnum Prioridad { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public string? Categoria { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int TotalAsignaciones { get; set; }
}
