using WebAPIUser.DTOs;
using WebAPIUser.Models;

namespace WebAPIUser.Services;

public interface IMapperService
{
    UsuarioDto ToDto(Usuario usuario);
    IEnumerable<UsuarioDto> ToDto(IEnumerable<Usuario> usuarios);
    Usuario ToEntity(CreateUsuarioDto dto);
    void MapUpdateDtoToEntity(UpdateUsuarioDto dto, Usuario entity);
}

public class MapperService : IMapperService
{
    public UsuarioDto ToDto(Usuario usuario)
    {
        return new UsuarioDto
        {
            Id = usuario.Id,
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            Correo = usuario.Correo,
            Username = usuario.Username,
            FechaCreacion = usuario.FechaCreacion
        };
    }

    public IEnumerable<UsuarioDto> ToDto(IEnumerable<Usuario> usuarios)
    {
        return usuarios.Select(ToDto);
    }

    public Usuario ToEntity(CreateUsuarioDto dto)
    {
        return new Usuario
        {
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Correo = dto.Correo,
            Username = dto.Username,
            FechaCreacion = DateTime.UtcNow
        };
    }

    public void MapUpdateDtoToEntity(UpdateUsuarioDto dto, Usuario entity)
    {
        entity.Nombres = dto.Nombres;
        entity.Apellidos = dto.Apellidos;
        entity.Correo = dto.Correo;
        entity.Username = dto.Username;
    }
}
