namespace WebAPIUser.Models;

/// <summary>
/// Tabla pivote para relación M:M entre Usuario y Tarea
/// </summary>
public class UsuarioTarea
{
    public int Id { get; set; }

    // FK hacia Usuario
    public int UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;

    // FK hacia Tarea
    public int TareaId { get; set; }
    public virtual Tarea Tarea { get; set; } = null!;

    // Campos propios de la relación
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    public EstadoTareaEnum Estado { get; set; } = EstadoTareaEnum.Pendiente;

    public DateTime? FechaCompletado { get; set; }
}

/// <summary>
/// Enum de estados de tarea asignada
/// </summary>
public enum EstadoTareaEnum
{
    Pendiente = 1,
    EnProgreso = 2,
    Completada = 3,
    Cancelada = 4
}