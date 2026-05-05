using Microsoft.EntityFrameworkCore;
using WebAPIUser.DTOs;
using WebAPIUser.Models;

namespace WebAPIUser.Services;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioDto>> GetAllUsuariosAsync();
    Task<UsuarioDto?> GetUsuarioByIdAsync(int id);
    Task<UsuarioDto> CreateUsuarioAsync(CreateUsuarioDto dto);
    Task<UsuarioDto?> UpdateUsuarioAsync(int id, UpdateUsuarioDto dto);
    Task<bool> DeleteUsuarioAsync(int id);
    Task<bool> UsuarioExisteAsync(int id);
}

public class UsuarioService : IUsuarioService
{
    private readonly DbUserContext _context;
    private readonly IMapperService _mapperService;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(DbUserContext context, IMapperService mapperService, ILogger<UsuarioService> logger)
    {
        _context = context;
        _mapperService = mapperService;
        _logger = logger;
    }

    public async Task<IEnumerable<UsuarioDto>> GetAllUsuariosAsync()
    {
        _logger.LogInformation("Obteniendo lista de todos los usuarios");
        var usuarios = await _context.Usuarios.ToListAsync();
        return _mapperService.ToDto(usuarios);
    }

    public async Task<UsuarioDto?> GetUsuarioByIdAsync(int id)
    {
        _logger.LogInformation("Buscando usuario con ID: {UsuarioId}", id);
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            _logger.LogWarning("Usuario no encontrado con ID: {UsuarioId}", id);
            return null;
        }

        return _mapperService.ToDto(usuario);
    }

    public async Task<UsuarioDto> CreateUsuarioAsync(CreateUsuarioDto dto)
    {
        _logger.LogInformation("Creando nuevo usuario: {Correo}", dto.Correo);

        var usuarioExistente = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == dto.Correo);

        if (usuarioExistente != null)
        {
            _logger.LogWarning("Intento de crear usuario con correo existente: {Correo}", dto.Correo);
            throw new InvalidOperationException($"Ya existe un usuario registrado con el correo: {dto.Correo}");
        }

        var usuario = _mapperService.ToEntity(dto);
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario creado exitosamente con ID: {UsuarioId}", usuario.Id);
        return _mapperService.ToDto(usuario);
    }

    public async Task<UsuarioDto?> UpdateUsuarioAsync(int id, UpdateUsuarioDto dto)
    {
        _logger.LogInformation("Actualizando usuario con ID: {UsuarioId}", id);

        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            _logger.LogWarning("Usuario no encontrado para actualizar con ID: {UsuarioId}", id);
            return null;
        }

        var usuarioConCorreo = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == dto.Correo && u.Id != id);

        if (usuarioConCorreo != null)
        {
            _logger.LogWarning("Intento de actualizar con correo duplicado: {Correo}", dto.Correo);
            throw new InvalidOperationException($"Ya existe otro usuario con el correo: {dto.Correo}");
        }

        _mapperService.MapUpdateDtoToEntity(dto, usuario);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario actualizado exitosamente con ID: {UsuarioId}", id);
        return _mapperService.ToDto(usuario);
    }

    public async Task<bool> DeleteUsuarioAsync(int id)
    {
        _logger.LogInformation("Eliminando usuario con ID: {UsuarioId}", id);

        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            _logger.LogWarning("Usuario no encontrado para eliminar con ID: {UsuarioId}", id);
            return false;
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario eliminado exitosamente con ID: {UsuarioId}", id);
        return true;
    }

    public async Task<bool> UsuarioExisteAsync(int id)
    {
        return await _context.Usuarios.AnyAsync(u => u.Id == id);
    }
}
