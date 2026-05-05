using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIUser.Models;

namespace WebAPIUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly DbUserContext _context;

        public UsuariosController(DbUserContext context)
        {
            _context = context;
        }

        [HttpGet("listar")]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuario()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return Ok(usuarios);//200
        }

        [HttpPost("guardar")]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created, usuario);
        }

        [HttpPut("actualizar/{id}")]
        public async Task<ActionResult> PutUsario(int id, Usuario usuario)
        {
            var usuarioUpdate = await _context.Usuarios.FindAsync(id);

            if(usuarioUpdate == null)
            {
                return NotFound();//404
            }

            usuarioUpdate.Nombres = usuario.Nombres;
            usuarioUpdate.Apellidos = usuario.Apellidos;
            usuarioUpdate.Correo = usuario.Correo;
            usuarioUpdate.Username = usuario.Username;

            await _context.SaveChangesAsync();

            return Ok(usuarioUpdate);
        }

        [HttpDelete("eliminar/{id}")]
        public async Task<ActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if(usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("buscar/{id}")]
        public async Task<ActionResult<Usuario>> SearchUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }
    }
}
