using Microsoft.AspNetCore.Mvc;
using WebAPIUser.DTOs;
using WebAPIUser.Services;

namespace WebAPIUser.Controllers;

/// <summary>
/// Proxy controller hacia PokéAPI (https://pokeapi.co).
/// El cliente React llama a este controller en lugar de llamar directamente
/// a PokéAPI, lo que permite:
/// - Centralizar el acceso a la API externa.
/// - Agregar caché, autenticación o lógica de negocio en el futuro.
/// - Combinar datos de Pokémon con entidades propias (usuarios, tareas).
/// - Evitar problemas de CORS desde el browser hacia dominios externos.
/// Base URL: <c>/api/Pokemon</c>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PokemonController : ControllerBase
{
    private readonly IPokemonService _pokemonService;
    private readonly ILogger<PokemonController> _logger;

    /// <summary>
    /// Inicializa el controller con sus dependencias inyectadas.
    /// </summary>
    /// <param name="pokemonService">Servicio proxy que encapsula las llamadas a PokéAPI.</param>
    /// <param name="logger">Logger para trazabilidad.</param>
    public PokemonController(IPokemonService pokemonService, ILogger<PokemonController> logger)
    {
        _pokemonService = pokemonService;
        _logger         = logger;
    }

    /// <summary>
    /// Returns a paginated list of Pokémon with name, ID and sprite image URL.
    /// </summary>
    /// <remarks>
    /// Proxies the PokéAPI endpoint: <c>GET /api/v2/pokemon?limit={limit}&amp;offset={offset}</c>
    ///
    /// Usage examples:
    /// - First 20 Pokémon: <c>GET /api/Pokemon/list</c>
    /// - Next 20: <c>GET /api/Pokemon/list?offset=20</c>
    /// - 50 per page: <c>GET /api/Pokemon/list?limit=50</c>
    ///
    /// Possible errors:
    /// - 400: limit or offset are negative.
    /// - 502: PokéAPI is unreachable or returned an unexpected error.
    /// </remarks>
    /// <param name="limit">Number of Pokémon per page (1–100). Default: 20.</param>
    /// <param name="offset">Number of Pokémon to skip from the start. Default: 0.</param>
    /// <returns>List of Pokémon summaries with name, ID and image URL.</returns>
    /// <response code="200">Returns the paginated list of Pokémon.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    /// <response code="502">Could not reach PokéAPI.</response>
    [HttpGet("list")]
    [ProducesResponseType(typeof(IEnumerable<PokemonSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<IEnumerable<PokemonSummaryDto>>> GetAll(
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        if (limit < 1 || limit > 100)
            return BadRequest("limit must be between 1 and 100.");

        if (offset < 0)
            return BadRequest("offset must be 0 or greater.");

        try
        {
            var pokemon = await _pokemonService.GetAllAsync(limit, offset);
            return Ok(pokemon);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach PokéAPI on list endpoint");
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Could not reach PokéAPI. Please try again later." });
        }
    }

    /// <summary>
    /// Returns the full detail of a single Pokémon by name or national Pokédex ID.
    /// </summary>
    /// <remarks>
    /// Proxies the PokéAPI endpoint: <c>GET /api/v2/pokemon/{nameOrId}</c>
    ///
    /// Usage examples:
    /// - By name: <c>GET /api/Pokemon/find/pikachu</c>
    /// - By ID:   <c>GET /api/Pokemon/find/25</c>
    ///
    /// Returns: ID, name, height, weight, sprite URL, types, abilities and base stats.
    ///
    /// Possible errors:
    /// - 404: Pokémon not found with the given name or ID.
    /// - 502: PokéAPI is unreachable or returned an unexpected error.
    /// </remarks>
    /// <param name="nameOrId">Pokémon name in lowercase (e.g. "pikachu") or national Pokédex ID (e.g. "25").</param>
    /// <returns>Full Pokémon detail including types, abilities and base stats.</returns>
    /// <response code="200">Returns the Pokémon detail.</response>
    /// <response code="404">Pokémon not found.</response>
    /// <response code="502">Could not reach PokéAPI.</response>
    [HttpGet("find/{nameOrId}")]
    [ProducesResponseType(typeof(PokemonDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PokemonDetailDto>> GetByNameOrId(string nameOrId)
    {
        try
        {
            var pokemon = await _pokemonService.GetByNameOrIdAsync(nameOrId);

            if (pokemon is null)
                return NotFound(new { message = $"Pokemon '{nameOrId}' was not found." });

            return Ok(pokemon);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach PokéAPI for: {NameOrId}", nameOrId);
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Could not reach PokéAPI. Please try again later." });
        }
    }
}
