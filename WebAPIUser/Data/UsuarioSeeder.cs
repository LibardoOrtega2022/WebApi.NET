using WebAPIUser.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace WebAPIUser.Data;

/// <summary>
/// Generador de datos de prueba para la entidad <see cref="Usuario"/>.
/// Crea 10 usuarios con nombres, apellidos, correos y usernames aleatorios
/// tomados de pools predefinidos de nombres hispanohablantes.
/// Garantiza que no se generen correos duplicados dentro del mismo lote.
/// Solo inserta si la tabla <c>Usuario</c> está vacía (idempotente).
/// </summary>
public static class UsuarioSeeder
{
    /// <summary>Instancia compartida de Random para todas las generaciones aleatorias.</summary>
    private static readonly Random _random = new();

    /// <summary>
    /// Pool de nombres de pila en español usados para generar usuarios de prueba.
    /// </summary>
    private static readonly string[] _nombres = new[]
    {
        "Juan", "María", "Carlos", "Ana", "Luis", "Patricia", "Roberto", "Laura", "Fernando", "Isabel",
        "José", "Carmen", "Francisco", "Dolores", "Manuel", "Rosa", "Miguel", "Marta", "Antonio", "Pilar"
    };

    /// <summary>
    /// Pool de apellidos en español usados para generar usuarios de prueba.
    /// </summary>
    private static readonly string[] _apellidos = new[]
    {
        "García", "Rodríguez", "Martínez", "Hernández", "López", "González", "Pérez", "Sánchez", 
        "Ramírez", "Torres", "Flores", "Rivera", "Morales", "Gutiérrez", "Ortiz", "Jiménez",
        "Reyes", "Cruz", "Vargas", "Castro"
    };

    /// <summary>
    /// Pool de dominios de correo electrónico usados al construir las direcciones de los usuarios.
    /// </summary>
    private static readonly string[] _dominios = new[]
    {
        "gmail.com",
        "outlook.com",
        "yahoo.com",
        "empresa.com",
        "mail.com",
        "example.com",
        "company.dev",
        "techcorp.io"
    };

    /// <summary>
    /// Genera una lista de usuarios únicos con correos no duplicados.
    /// Usa un bucle do-while para regenerar el usuario si el correo ya fue usado en el lote.
    /// </summary>
    /// <param name="cantidad">Número de usuarios a generar. Por defecto 10.</param>
    /// <returns>Lista de entidades <see cref="Usuario"/> sin ID (se asigna al persistir).</returns>
    public static List<Usuario> GenerarUsuarios(int cantidad = 10)
    {
        var usuarios = new List<Usuario>();
        var correosUsados = new HashSet<string>();

        for (int i = 0; i < cantidad; i++)
        {
            Usuario usuario;
            string correoTemporal;

            // Garantizar correos únicos
            do
            {
                usuario = GenerarUsuarioAleatorio(i + 1);
                correoTemporal = usuario.Correo;
            } while (correosUsados.Contains(correoTemporal));

            correosUsados.Add(correoTemporal);
            usuarios.Add(usuario);
        }

        return usuarios;
    }

    /// <summary>
    /// Genera un único usuario con todos sus campos poblados aleatoriamente.
    /// La fecha de creación se retrocede hasta 90 días para simular usuarios existentes.
    /// </summary>
    /// <param name="numero">Número secuencial usado en el username para garantizar unicidad.</param>
    /// <returns>Entidad <see cref="Usuario"/> lista para persistir.</returns>
    private static Usuario GenerarUsuarioAleatorio(int numero)
    {
        var nombre = _nombres[_random.Next(_nombres.Length)];
        var apellido = _apellidos[_random.Next(_apellidos.Length)];
        var dominio = _dominios[_random.Next(_dominios.Length)];

        return new Usuario
        {
            Nombres = nombre,
            Apellidos = apellido,
            Correo = GenerarCorreoUnico(nombre, apellido, dominio),
            Username = GenerarUsername(nombre, apellido, numero),
            FechaCreacion = DateTime.UtcNow.AddDays(-_random.Next(90))
        };
    }

    /// <summary>
    /// Construye una dirección de correo en formato <c>nombre.apellido@dominio</c>
    /// con todos los caracteres en minúsculas.
    /// Ejemplo: "juan.garcia@gmail.com".
    /// </summary>
    /// <param name="nombre">Nombre de pila del usuario.</param>
    /// <param name="apellido">Apellido del usuario.</param>
    /// <param name="dominio">Dominio de correo seleccionado aleatoriamente.</param>
    /// <returns>Dirección de correo en minúsculas.</returns>
    private static string GenerarCorreoUnico(string nombre, string apellido, string dominio)
    {
        var basCorr = $"{nombre.ToLower()}.{apellido.ToLower()}@{dominio}";
        return basCorr;
    }

    /// <summary>
    /// Construye un nombre de usuario en formato <c>nombreapellidonumero</c> en minúsculas.
    /// El número secuencial garantiza que dos usuarios con el mismo nombre y apellido
    /// tengan usernames distintos. Ejemplo: "juangarcia3".
    /// </summary>
    /// <param name="nombre">Nombre de pila del usuario.</param>
    /// <param name="apellido">Apellido del usuario.</param>
    /// <param name="numero">Número secuencial del usuario en el lote.</param>
    /// <returns>Username en minúsculas con número al final.</returns>
    private static string GenerarUsername(string nombre, string apellido, int numero)
    {
        var username = $"{nombre.ToLower()}{apellido.ToLower()}{numero}";
        return username;
    }

    /// <summary>
    /// Verifica si la tabla <c>Usuario</c> ya tiene registros.
    /// Si está vacía, genera 10 usuarios únicos y los inserta en bloque.
    /// Imprime en consola el progreso y la lista completa de usuarios creados.
    /// </summary>
    /// <param name="context">Contexto de base de datos activo.</param>
    public static async Task InicializarUsuariosAsync(DbUserContext context)
    {
        // Verificar si ya existen usuarios
        if (await context.Usuarios.AsQueryable().AnyAsync())
        {
            var cantidadUsuarios = await context.Usuarios.AsQueryable().CountAsync();
            Console.WriteLine($"✅ Ya existen {cantidadUsuarios} usuarios en la base de datos");
            return;
        }

        Console.WriteLine("🔄 Generando 10 usuarios aleatorios...");

        try
        {
            var usuarios = GenerarUsuarios(10);

            await context.Usuarios.AddRangeAsync(usuarios);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Se crearon {usuarios.Count} usuarios exitosamente\n");

            // Mostrar muestra de usuarios creados
            Console.WriteLine("📋 Usuarios creados:");
            foreach (var usuario in usuarios)
            {
                Console.WriteLine($"   • {usuario.Nombres} {usuario.Apellidos} ({usuario.Correo})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al crear usuarios: {ex.Message}");
            throw;
        }
    }
}
