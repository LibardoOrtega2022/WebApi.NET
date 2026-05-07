using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebAPIUser.Models;

/// <summary>
/// Contexto principal de Entity Framework Core para la aplicación.
/// Actúa como puente entre las entidades C# y las tablas de SQL Server.
/// Registra los <see cref="DbSet{T}"/> y configura el mapeo columna-propiedad
/// mediante Fluent API en <see cref="OnModelCreating"/>.
/// </summary>
public partial class DbUserContext : DbContext
{
    /// <summary>
    /// Constructor sin parámetros requerido por EF Core para migraciones y scaffolding.
    /// </summary>
    public DbUserContext()
    {
    }

    /// <summary>
    /// Constructor principal usado en tiempo de ejecución.
    /// Recibe las opciones de configuración (cadena de conexión, proveedor, etc.)
    /// inyectadas desde <c>Program.cs</c>.
    /// </summary>
    /// <param name="options">Opciones de configuración del contexto.</param>
    public DbUserContext(DbContextOptions<DbUserContext> options)
        : base(options)
    {
    }

    /// <summary>Tabla de usuarios. Mapeada a <c>Usuario</c> en la base de datos.</summary>
    public virtual DbSet<Usuario> Usuarios { get; set; }

    /// <summary>Tabla de tareas. Mapeada a <c>Tarea</c> en la base de datos.</summary>
    public virtual DbSet<Tarea> Tareas { get; set; }

    /// <summary>
    /// Tabla pivote de asignaciones usuario-tarea.
    /// Mapeada a <c>UsuarioTarea</c> en la base de datos.
    /// </summary>
    public virtual DbSet<UsuarioTarea> UsuarioTareas { get; set; }

    /// <summary>
    /// Configura el esquema de la base de datos mediante Fluent API.
    /// Define nombres de tabla, nombres de columna (snake_case), tipos de dato,
    /// longitudes máximas y relaciones con sus comportamientos de eliminación en cascada.
    /// Este método se ejecuta una sola vez al inicializar el contexto.
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo usado para configurar entidades.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Configuración de Usuario ──────────────────────────────────────────
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3213E83FA0173B0D");

            entity.ToTable("Usuario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("apellidos");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.FechaCreacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombres)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombres");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        // Configuración de Tarea
        modelBuilder.Entity<Tarea>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Tarea");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Titulo)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("titulo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Prioridad)
                .HasColumnName("prioridad");
            entity.Property(e => e.Categoria)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("categoria");
            entity.Property(e => e.FechaVencimiento)
                .HasColumnType("datetime")
                .HasColumnName("fecha_vencimiento");
            entity.Property(e => e.FechaCreacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
        });

        // Configuración de UsuarioTarea (Tabla Pivote)
        modelBuilder.Entity<UsuarioTarea>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("UsuarioTarea");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.TareaId).HasColumnName("tarea_id");
            entity.Property(e => e.FechaAsignacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_asignacion");
            entity.Property(e => e.Estado)
                .HasColumnName("estado");
            entity.Property(e => e.FechaCompletado)
                .HasColumnType("datetime")
                .HasColumnName("fecha_completado");

            // Relaciones
            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.UsuarioTareas)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tarea)
                .WithMany(t => t.UsuarioTareas)
                .HasForeignKey(e => e.TareaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    /// <summary>
    /// Punto de extensión parcial para configuraciones adicionales del modelo.
    /// Implementado en archivos parciales generados por scaffolding si aplica.
    /// </summary>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
