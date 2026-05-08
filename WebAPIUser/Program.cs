// ─────────────────────────────────────────────────────────────────────────────
// WebAPIUser — Entry point de la aplicación ASP.NET Core
// Configura el contenedor de dependencias (builder) y el pipeline HTTP (app).
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Reflection;
using WebAPIUser.Data;
using WebAPIUser.Middleware;
using WebAPIUser.Models;
using WebAPIUser.Services;

var builder = WebApplication.CreateBuilder(args);

// ── CORS ──────────────────────────────────────────────────────────────────────
// Lee los orígenes permitidos desde appsettings.json / appsettings.Development.json.
// Esto permite que el frontend React (Vite en puerto 5173) llame al backend
// sin que el browser bloquee la petición por política de mismo origen.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

const string CorsPolicyName = "ReactFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            // Solo permite los orígenes configurados (no wildcard en producción)
            .WithOrigins(allowedOrigins)
            // Permite los métodos HTTP usados por la API REST
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            // Permite los headers estándar de JSON y autenticación
            .WithHeaders("Content-Type", "Authorization", "Accept")
            // Permite que el browser envíe cookies/credenciales si se necesita en el futuro
            .AllowCredentials();
    });
});

// ── Base de datos ─────────────────────────────────────────────────────────────
// Registra DbUserContext con SQL Server usando la cadena de conexión "connectionDB"
// definida en appsettings.json.
builder.Services.AddDbContext<DbUserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("connectionDB")));

// ── HttpClient para PokéAPI (Opción B: proxy) ─────────────────────────────────
// Registra un HttpClient nombrado "PokeApi" con la base URL de PokéAPI.
// El servicio PokemonService lo resuelve por nombre desde IHttpClientFactory.
// Timeout de 10 segundos para no bloquear el thread si PokéAPI tarda.
builder.Services.AddHttpClient("PokeApi", client =>
{
    var baseUrl = builder.Configuration["PokeApi:BaseUrl"]
                  ?? "https://pokeapi.co/api/v2/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout     = TimeSpan.FromSeconds(10);
    // Header recomendado por PokéAPI para identificar el cliente
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ── Servicios de negocio (Scoped: una instancia por request) ───────────────────
builder.Services.AddScoped<IMapperService, MapperService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ITareaService, TareaService>();
// PokemonService es Scoped porque usa HttpClient (stateless, pero sigue el patrón)
builder.Services.AddScoped<IPokemonService, PokemonService>();

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
// AddOpenApi() registra el soporte nativo de .NET 10 para OpenAPI.
// AddSwaggerGen() registra Swashbuckle para generar la UI interactiva.
// Se llama dos veces: la primera sin opciones (requerida por el pipeline),
// la segunda con la configuración personalizada del documento.
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    // Metadatos del documento OpenAPI visible en la UI de Swagger
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WebAPIUser + Pokemon Proxy",
        Version = "v1",
        Description = "REST API for user/task management and PokéAPI proxy for the React SPA frontend.",
        Contact = new OpenApiContact
        {
            Name  = "Libardo Amesquita",
            Email = "libardoadolfo2@gmail.com"
        }
    });

    // El archivo XML se genera automáticamente gracias a <GenerateDocumentationFile>true</GenerateDocumentationFile>
    // en el .csproj, y contiene todos los /// <summary> del código.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ── Middleware global de excepciones ──────────────────────────────────────────
// Debe registrarse PRIMERO para interceptar cualquier excepción del pipeline.
// Convierte excepciones no manejadas en respuestas JSON estructuradas.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ── CORS ──────────────────────────────────────────────────────────────────────
// UseCors DEBE ir antes de UseAuthorization y MapControllers
// para que los headers CORS se agreguen antes de que el pipeline procese la ruta.
app.UseCors(CorsPolicyName);

// ── Redirección raíz → Swagger ────────────────────────────────────────────────
// Redirige la URL raíz "/" a "/swagger" para facilitar el acceso a la documentación.
app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// ── Pipeline HTTP ─────────────────────────────────────────────────────────────
// Solo en Development se expone Swagger para no publicar la documentación en producción.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI();
    app.UseSwagger();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// ── Inicialización de base de datos ───────────────────────────────────────────
// Se ejecuta al arrancar la aplicación:
// 1. MigrateAsync() aplica las migraciones pendientes automáticamente.
// 2. InicializarDatosAsync() inserta datos de prueba si las tablas están vacías.
// El scope se crea manualmente porque DbContext es Scoped y no puede resolverse
// directamente desde el contenedor raíz (Singleton).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbUserContext>();

    // Asegurar que las migraciones están aplicadas
    await db.Database.MigrateAsync();

    // Llenar datos de prueba si es necesario
    await DataSeeder.InicializarDatosAsync(db);
}

app.Run();
