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
        /// Obtiene la lista de todos los usuarios.
        /// <br/>Retrieves the list of all users.
        /// </summary>
        /// <returns>
        /// Lista de usuarios registrados en el sistema.
        /// <br/>List of users registered in the system.
        /// </returns>
        /// <response code="200">
        /// Retorna la lista de usuarios.
        /// <br/>Returns the list of users.
        /// </response>
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            var usuarios = await _usuarioService.GetAllUsuariosAsync();
            return Ok(usuarios);
        }

        /// <summary>
        /// Busca un usuario por su ID.
        /// <br/>Finds a user by their ID.
        /// </summary>
        /// <param name="id">
        /// ID del usuario a buscar (debe ser mayor a 0).
        /// <br/>ID of the user to find (must be greater than 0).
        /// </param>
        /// <returns>
        /// El usuario encontrado con todos sus datos.
        /// <br/>The found user with all their data.
        /// </returns>
        /// <response code="200">
        /// Retorna el usuario encontrado.
        /// <br/>Returns the found user.
        /// </response>
        /// <response code="400">
        /// ID inválido (menor o igual a 0).
        /// <br/>Invalid ID (less than or equal to 0).
        /// </response>
        /// <response code="404">
        /// Usuario no encontrado con el ID proporcionado.
        /// <br/>User not found with the provided ID.
        /// </response>
        [HttpGet("find/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        /// Crea un nuevo usuario en el sistema.
        /// <br/>Creates a new user in the system.
        /// </summary>
        /// <param name="createDto">
        /// Datos del usuario a crear (nombre, correo, contraseña).
        /// <br/>Data of the user to create (name, email, password).
        /// </param>
        /// <returns>
        /// El usuario creado con su ID autogenerado.
        /// <br/>The created user with their auto-generated ID.
        /// </returns>
        /// <response code="201">
        /// Usuario creado exitosamente. Incluye la ubicación del recurso en el header Location.
        /// <br/>User created successfully. Includes the resource location in the Location header.
        /// </response>
        /// <response code="400">
        /// Datos inválidos o validaciones de modelo no cumplidas.
        /// <br/>Invalid data or model validation failed.
        /// </response>
        /// <response code="409">
        /// El correo electrónico ya está registrado en el sistema.
        /// <br/>The email address is already registered in the system.
        /// </response>
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
        /// Actualiza un usuario existente de forma completa (PUT).
        /// <br/>Fully updates an existing user (PUT).
        /// </summary>
        /// <param name="id">
        /// ID del usuario a actualizar (debe ser mayor a 0).
        /// <br/>ID of the user to update (must be greater than 0).
        /// </param>
        /// <param name="updateDto">
        /// Datos actualizados del usuario (nombre, correo, contraseña).
        /// <br/>Updated user data (name, email, password).
        /// </param>
        /// <returns>
        /// El usuario actualizado con todos sus campos modificados.
        /// <br/>The updated user with all modified fields.
        /// </returns>
        /// <response code="200">
        /// Usuario actualizado exitosamente.
        /// <br/>User updated successfully.
        /// </response>
        /// <response code="400">
        /// Datos inválidos o validaciones de modelo no cumplidas.
        /// <br/>Invalid data or model validation failed.
        /// </response>
        /// <response code="404">
        /// Usuario no encontrado con el ID proporcionado.
        /// <br/>User not found with the provided ID.
        /// </response>
        /// <response code="409">
        /// El correo electrónico ya está registrado por otro usuario.
        /// <br/>The email address is already registered by another user.
        /// </response>
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
        /// Elimina un usuario del sistema de forma permanente.
        /// <br/>Permanently deletes a user from the system.
        /// </summary>
        /// <param name="id">
        /// ID del usuario a eliminar (debe ser mayor a 0).
        /// <br/>ID of the user to delete (must be greater than 0).
        /// </param>
        /// <returns>
        /// Sin contenido si la eliminación fue exitosa.
        /// <br/>No content if the deletion was successful.
        /// </returns>
        /// <response code="204">
        /// Usuario eliminado exitosamente. No retorna contenido.
        /// <br/>User deleted successfully. No content returned.
        /// </response>
        /// <response code="400">
        /// ID inválido (menor o igual a 0).
        /// <br/>Invalid ID (less than or equal to 0).
        /// </response>
        /// <response code="404">
        /// Usuario no encontrado con el ID proporcionado.
        /// <br/>User not found with the provided ID.
        /// </response>
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
