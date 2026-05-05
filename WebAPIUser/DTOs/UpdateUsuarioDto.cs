using System.ComponentModel.DataAnnotations;

namespace WebAPIUser.DTOs;

public class UpdateUsuarioDto
{
    [Required(ErrorMessage = "Los nombres son requeridos")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 50 caracteres")]
    public string Nombres { get; set; } = null!;

    [Required(ErrorMessage = "Los apellidos son requeridos")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 50 caracteres")]
    public string Apellidos { get; set; } = null!;

    [Required(ErrorMessage = "El correo es requerido")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
    [StringLength(100)]
    public string Correo { get; set; } = null!;

    [StringLength(100, ErrorMessage = "El nombre de usuario no puede exceder 100 caracteres")]
    public string? Username { get; set; }
}
