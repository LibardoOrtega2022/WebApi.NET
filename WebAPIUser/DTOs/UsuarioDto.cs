namespace WebAPIUser.DTOs;

public class UsuarioDto
{
    public int Id { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string? Username { get; set; }

    public DateTime? FechaCreacion { get; set; }
}
