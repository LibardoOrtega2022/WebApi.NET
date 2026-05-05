using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPIUser.DTOs;
using WebAPIUser.Services;

namespace WebAPIUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(IUsuarioService usuarioService, ILogger<UsuariosController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista de todos los usuarios
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        /// <response code="200">Retorna la lista de usuarios</response>
        [HttpGet("listar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            var usuarios = await _usuarioService.GetAllUsuariosAsync();
            return Ok(usuarios);
        }

        /// <summary>
        /// Busca un usuario por su ID
        /// </summary>
        /// <param name="id">ID del usuario a buscar</param>
        /// <returns>Usuario encontrado</returns>
        /// <response code="200">Retorna el usuario encontrado</response>
        /// <response code="404">Usuario no encontrado</response>
        [HttpGet("buscar/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioDto>> GetUsuarioById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("El ID debe ser mayor a 0");
            }

            var usuario = await _usuarioService.GetUsuarioByIdAsync(id);

            if (usuario == null)
            {
                return NotFound($"No se encontró usuario con ID: {id}");
            }

            return Ok(usuario);
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        /// <param name="createDto">Datos del usuario a crear</param>
        /// <returns>Usuario creado</returns>
        /// <response code="201">Usuario creado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="409">El correo ya está registrado</response>
        [HttpPost("guardar")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UsuarioDto>> PostUsuario([FromBody] CreateUsuarioDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var usuarioCreado = await _usuarioService.CreateUsuarioAsync(createDto);
                return CreatedAtAction(nameof(GetUsuarioById), new { id = usuarioCreado.Id }, usuarioCreado);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        /// <param name="id">ID del usuario a actualizar</param>
        /// <param name="updateDto">Datos actualizados del usuario</param>
        /// <returns>Usuario actualizado</returns>
        /// <response code="200">Usuario actualizado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="409">El correo ya está registrado</response>
        [HttpPut("actualizar/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UsuarioDto>> PutUsuario(int id, [FromBody] UpdateUsuarioDto updateDto)
        {
            if (id <= 0)
            {
                return BadRequest("El ID debe ser mayor a 0");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var usuarioActualizado = await _usuarioService.UpdateUsuarioAsync(id, updateDto);

                if (usuarioActualizado == null)
                {
                    return NotFound($"No se encontró usuario con ID: {id}");
                }

                return Ok(usuarioActualizado);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        /// <param name="id">ID del usuario a eliminar</param>
        /// <returns>No retorna contenido</returns>
        /// <response code="204">Usuario eliminado exitosamente</response>
        /// <response code="400">ID inválido</response>
        /// <response code="404">Usuario no encontrado</response>
        [HttpDelete("eliminar/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUsuario(int id)
        {
            if (id <= 0)
            {
                return BadRequest("El ID debe ser mayor a 0");
            }

            var resultado = await _usuarioService.DeleteUsuarioAsync(id);

            if (!resultado)
            {
                return NotFound($"No se encontró usuario con ID: {id}");
            }

            return NoContent();
        }
    }
}
