namespace WebAPIUser.DTOs;

// ─────────────────────────────────────────────────────────────────────────────
// DTOs para el proxy de PokéAPI.
// Estos objetos representan únicamente los campos que el backend expone al
// cliente React. Se mapean desde las respuestas JSON de pokeapi.co.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Resumen de un Pokémon devuelto en listados.
/// Contiene solo nombre e ID extraído de la URL de PokéAPI.
/// </summary>
public class PokemonSummaryDto
{
    /// <summary>Nombre en minúsculas del Pokémon (ej. "bulbasaur").</summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// ID numérico extraído de la URL de PokéAPI.
    /// Ejemplo: de "https://pokeapi.co/api/v2/pokemon/1/" se extrae 1.
    /// </summary>
    public int Id { get; set; }

    /// <summary>URL de la imagen oficial del Pokémon (sprite frontal por defecto).</summary>
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Detalle completo de un Pokémon individual.
/// Incluye estadísticas base, tipos, habilidades y sprites.
/// </summary>
public class PokemonDetailDto
{
    /// <summary>ID numérico único del Pokémon en la Pokédex nacional.</summary>
    public int Id { get; set; }

    /// <summary>Nombre en minúsculas del Pokémon.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Altura del Pokémon en decímetros.</summary>
    public int Height { get; set; }

    /// <summary>Peso del Pokémon en hectogramos.</summary>
    public int Weight { get; set; }

    /// <summary>URL del sprite frontal por defecto.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Lista de tipos del Pokémon (ej. ["fire", "flying"]).</summary>
    public List<string> Types { get; set; } = [];

    /// <summary>Lista de habilidades del Pokémon (ej. ["blaze", "solar-power"]).</summary>
    public List<string> Abilities { get; set; } = [];

    /// <summary>Estadísticas base del Pokémon (HP, Attack, Defense, etc.).</summary>
    public List<PokemonStatDto> Stats { get; set; } = [];
}

/// <summary>
/// Estadística base individual de un Pokémon.
/// </summary>
public class PokemonStatDto
{
    /// <summary>Nombre de la estadística (ej. "hp", "attack", "speed").</summary>
    public string Name { get; set; } = null!;

    /// <summary>Valor base de la estadística.</summary>
    public int Value { get; set; }
}

/// <summary>
/// Respuesta paginada de PokéAPI para el listado de Pokémon.
/// Usado internamente por el servicio para deserializar la respuesta cruda.
/// </summary>
public class PokeApiListResponse
{
    /// <summary>Total de Pokémon disponibles en PokéAPI.</summary>
    public int Count { get; set; }

    /// <summary>URL de la siguiente página, o null si es la última.</summary>
    public string? Next { get; set; }

    /// <summary>URL de la página anterior, o null si es la primera.</summary>
    public string? Previous { get; set; }

    /// <summary>Lista de resultados de la página actual (nombre + URL).</summary>
    public List<PokeApiNamedResource> Results { get; set; } = [];
}

/// <summary>
/// Recurso nombrado de PokéAPI: par nombre/URL usado en listados.
/// </summary>
public class PokeApiNamedResource
{
    /// <summary>Nombre del recurso (ej. "bulbasaur").</summary>
    public string Name { get; set; } = null!;

    /// <summary>URL completa del recurso en PokéAPI.</summary>
    public string Url { get; set; } = null!;
}

/// <summary>
/// Respuesta cruda de PokéAPI para el detalle de un Pokémon.
/// Solo incluye los campos que el proxy necesita para construir <see cref="PokemonDetailDto"/>.
/// </summary>
public class PokeApiPokemonResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Height { get; set; }
    public int Weight { get; set; }
    public PokeApiSprites Sprites { get; set; } = null!;
    public List<PokeApiTypeSlot> Types { get; set; } = [];
    public List<PokeApiAbilitySlot> Abilities { get; set; } = [];
    public List<PokeApiStatSlot> Stats { get; set; } = [];
}

/// <summary>Sprites disponibles para un Pokémon en PokéAPI.</summary>
public class PokeApiSprites
{
    /// <summary>URL del sprite frontal por defecto (puede ser null para Pokémon sin sprite).</summary>
    public string? Front_Default { get; set; }
}

/// <summary>Slot de tipo de un Pokémon (incluye el slot numérico y el tipo).</summary>
public class PokeApiTypeSlot
{
    public int Slot { get; set; }
    public PokeApiNamedResource Type { get; set; } = null!;
}

/// <summary>Slot de habilidad de un Pokémon.</summary>
public class PokeApiAbilitySlot
{
    public bool Is_Hidden { get; set; }
    public PokeApiNamedResource Ability { get; set; } = null!;
}

/// <summary>Slot de estadística base de un Pokémon.</summary>
public class PokeApiStatSlot
{
    public int Base_Stat { get; set; }
    public PokeApiNamedResource Stat { get; set; } = null!;
}
