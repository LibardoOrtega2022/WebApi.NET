using System.ComponentModel.DataAnnotations;

namespace WebAPIUser.Models;

public partial class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Los nombres son requeridos")]
    [StringLength(50, MinimumLength = 2)]
    public string Nombres { get; set; } = null!;

    [Required(ErrorMessage = "Los apellidos son requeridos")]
    [StringLength(50, MinimumLength = 2)]
    public string Apellidos { get; set; } = null!;

    [Required(ErrorMessage = "El correo es requerido")]
    [EmailAddress]
    [StringLength(100)]
    public string Correo { get; set; } = null!;

    [StringLength(100)]
    public string? Username { get; set; }

    public DateTime? FechaCreacion { get; set; } = DateTime.UtcNow;
}
