using System.ComponentModel.DataAnnotations;
using WebAPIUser.Models;

namespace WebAPIUser.DTOs;

/// <summary>
/// DTO para crear una nueva tarea
/// </summary>
public class CreateTareaDto
{
    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 200 caracteres")]
    public string Titulo { get; set; } = null!;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La prioridad es requerida")]
    public PrioridadEnum Prioridad { get; set; } = PrioridadEnum.Media;

    public DateTime? FechaVencimiento { get; set; }

    [StringLength(100)]
    public string? Categoria { get; set; }
}
