using WebAPIUser.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace WebAPIUser.Data;

/// <summary>
/// Generador de datos de prueba para Usuarios
/// Crea 10 usuarios aleatorios con datos realistas
/// </summary>
public static class UsuarioSeeder
{
    private static readonly Random _random = new();

    private static readonly string[] _nombres = new[]
    {
        "Juan", "María", "Carlos", "Ana", "Luis", "Patricia", "Roberto", "Laura", "Fernando", "Isabel",
        "José", "Carmen", "Francisco", "Dolores", "Manuel", "Rosa", "Miguel", "Marta", "Antonio", "Pilar"
    };

    private static readonly string[] _apellidos = new[]
    {
        "García", "Rodríguez", "Martínez", "Hernández", "López", "González", "Pérez", "Sánchez", 
        "Ramírez", "Torres", "Flores", "Rivera", "Morales", "Gutiérrez", "Ortiz", "Jiménez",
        "Reyes", "Cruz", "Vargas", "Castro"
    };

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
    /// Genera 10 usuarios únicos aleatorios
    /// </summary>
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
    /// Genera un usuario individual aleatorio
    /// </summary>
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
    /// Genera un correo electrónico único
    /// </summary>
    private static string GenerarCorreoUnico(string nombre, string apellido, string dominio)
    {
        var basCorr = $"{nombre.ToLower()}.{apellido.ToLower()}@{dominio}";
        return basCorr;
    }

    /// <summary>
    /// Genera un nombre de usuario único
    /// </summary>
    private static string GenerarUsername(string nombre, string apellido, int numero)
    {
        var username = $"{nombre.ToLower()}{apellido.ToLower()}{numero}";
        return username;
    }

    /// <summary>
    /// Inicializa la base de datos con usuarios de prueba
    /// Crea 10 usuarios si no existen
    /// </summary>
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
