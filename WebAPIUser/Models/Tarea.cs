using System.ComponentModel.DataAnnotations;

namespace WebAPIUser.Models;

public class Tarea
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public PrioridadEnum Prioridad { get; set; }

    public string? Categoria { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Navegación hacia la tabla pivote
    public virtual ICollection<UsuarioTarea> UsuarioTareas { get; set; } = [];
}

/// <summary>
/// Enum de prioridades de tarea
/// </summary>
public enum PrioridadEnum
{
    Baja = 1,
    Media = 2,
    Alta = 3,
    Critica = 4
}
