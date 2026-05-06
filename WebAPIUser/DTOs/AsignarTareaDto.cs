namespace WebAPIUser.DTOs;

/// <summary>
/// DTO para asignar tarea a usuario
/// </summary>
public class AsignarTareaDto
{
    public int UsuarioId { get; set; }

    public int TareaId { get; set; }
}
