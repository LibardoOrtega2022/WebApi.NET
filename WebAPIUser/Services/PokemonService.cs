using System.Net.Http.Json;
using System.Text.Json;
using WebAPIUser.DTOs;

namespace WebAPIUser.Services;

/// <summary>
/// Contrato para el servicio proxy de PokéAPI.
/// Define las operaciones disponibles para consultar Pokémon desde el backend.
/// </summary>
public interface IPokemonService
{
    /// <summary>
    /// Obtiene una página de Pokémon del listado general.
    /// </summary>
    /// <param name="limit">Cantidad de Pokémon por página. Por defecto 20.</param>
    /// <param name="offset">Desplazamiento desde el inicio del listado. Por defecto 0.</param>
    /// <returns>Lista de resúmenes de Pokémon con nombre, ID e imagen.</returns>
    Task<IEnumerable<PokemonSummaryDto>> GetAllAsync(int limit = 20, int offset = 0);

    /// <summary>
    /// Obtiene el detalle completo de un Pokémon por su nombre o ID.
    /// </summary>
    /// <param name="nameOrId">Nombre en minúsculas (ej. "pikachu") o ID numérico (ej. "25").</param>
    /// <returns>Detalle del Pokémon, o <c>null</c> si no existe.</returns>
    Task<PokemonDetailDto?> GetByNameOrIdAsync(string nameOrId);
}

/// <summary>
/// Implementación del proxy hacia PokéAPI (https://pokeapi.co).
/// Actúa como intermediario entre el cliente React y la API externa:
/// realiza las llamadas HTTP, deserializa las respuestas y las transforma
/// en DTOs propios del proyecto, ocultando la estructura interna de PokéAPI.
///
/// Usa <see cref="HttpClient"/> registrado como cliente nombrado "PokeApi"
/// con la base URL configurada en appsettings.json.
/// </summary>
public class PokemonService : IPokemonService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PokemonService> _logger;

    // Opciones de deserialización JSON: ignora mayúsculas/minúsculas en nombres de propiedad.
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Inicializa el servicio con el HttpClient nombrado "PokeApi" y el logger.
    /// </summary>
    /// <param name="httpClientFactory">Factory para crear clientes HTTP nombrados.</param>
    /// <param name="logger">Logger para trazabilidad de llamadas y errores.</param>
    public PokemonService(IHttpClientFactory httpClientFactory, ILogger<PokemonService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("PokeApi");
        _logger     = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PokemonSummaryDto>> GetAllAsync(int limit = 20, int offset = 0)
    {
        _logger.LogInformation("Fetching Pokemon list — limit: {Limit}, offset: {Offset}", limit, offset);

        // Llama al endpoint de listado paginado de PokéAPI
        var response = await _httpClient.GetFromJsonAsync<PokeApiListResponse>(
            $"pokemon?limit={limit}&offset={offset}",
            _jsonOptions
        );

        if (response?.Results is null)
            return [];

        // Transforma cada resultado (nombre + URL) en un PokemonSummaryDto
        // extrayendo el ID desde la URL y construyendo la URL del sprite
        var summaries = response.Results.Select(r =>
        {
            var id = ExtractIdFromUrl(r.Url);
            return new PokemonSummaryDto
            {
                Name     = r.Name,
                Id       = id,
                ImageUrl = BuildSpriteUrl(id)
            };
        });

        _logger.LogInformation("Returned {Count} Pokemon summaries", response.Results.Count);
        return summaries;
    }

    /// <inheritdoc/>
    public async Task<PokemonDetailDto?> GetByNameOrIdAsync(string nameOrId)
    {
        _logger.LogInformation("Fetching Pokemon detail for: {NameOrId}", nameOrId);

        try
        {
            var raw = await _httpClient.GetFromJsonAsync<PokeApiPokemonResponse>(
                $"pokemon/{nameOrId.ToLower()}",
                _jsonOptions
            );

            if (raw is null)
                return null;

            // Mapea la respuesta cruda de PokéAPI al DTO propio del proyecto
            return new PokemonDetailDto
            {
                Id       = raw.Id,
                Name     = raw.Name,
                Height   = raw.Height,
                Weight   = raw.Weight,
                ImageUrl = raw.Sprites?.Front_Default,
                Types    = raw.Types
                              .OrderBy(t => t.Slot)
                              .Select(t => t.Type.Name)
                              .ToList(),
                Abilities = raw.Abilities
                               .Select(a => a.Ability.Name)
                               .ToList(),
                Stats    = raw.Stats
                              .Select(s => new PokemonStatDto
                              {
                                  Name  = s.Stat.Name,
                                  Value = s.Base_Stat
                              })
                              .ToList()
            };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // PokéAPI retorna 404 si el nombre/ID no existe — lo tratamos como null
            _logger.LogWarning("Pokemon not found: {NameOrId}", nameOrId);
            return null;
        }
    }

    // ── Helpers privados ──────────────────────────────────────────────────────

    /// <summary>
    /// Extrae el ID numérico desde una URL de PokéAPI.
    /// Ejemplo: "https://pokeapi.co/api/v2/pokemon/25/" → 25.
    /// </summary>
    /// <param name="url">URL completa del recurso en PokéAPI.</param>
    /// <returns>ID numérico, o 0 si no se puede parsear.</returns>
    private static int ExtractIdFromUrl(string url)
    {
        // La URL termina en "/{id}/" — tomamos el penúltimo segmento
        var segments = url.TrimEnd('/').Split('/');
        return int.TryParse(segments.LastOrDefault(), out var id) ? id : 0;
    }

    /// <summary>
    /// Construye la URL del sprite oficial frontal de un Pokémon
    /// usando el CDN de sprites de PokéAPI.
    /// </summary>
    /// <param name="id">ID numérico del Pokémon.</param>
    /// <returns>URL del sprite PNG.</returns>
    private static string BuildSpriteUrl(int id) =>
        $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{id}.png";
}
