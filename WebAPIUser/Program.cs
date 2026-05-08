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
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ── Base de datos ─────────────────────────────────────────────────────────────
// Registra DbUserContext con SQL Server usando la cadena de conexión "connectionDB"
// definida en appsettings.json.
builder.Services.AddDbContext<DbUserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("connectionDB")));

// Registrar servicios
// Scoped: una instancia por request HTTP. Apropiado para servicios que usan DbContext.
builder.Services.AddScoped<IMapperService, MapperService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ITareaService, TareaService>();

// Add services to the container.
builder.Services.AddControllers();

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
// AddOpenApi() registra el soporte nativo de .NET 10 para OpenAPI.
// AddSwaggerGen() configura Swashbuckle con un único documento "v1".
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    // Metadatos del documento OpenAPI visible en la UI de Swagger
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mi API de Usuarios/My User API",
        Version = "v1",
        Description = "API REST para gestión de usuarios y tareas/REST API for user and task management",
        Contact = new OpenApiContact
        {
            Name = "Libardo Amesquita",
            Email = "libardoadolfo2@gmail.com"
        }
    });

    // Incluye TODOS los endpoints en el documento "v1" independientemente del GroupName.
    // Sin esto, Swashbuckle filtra por GroupName y excluye los endpoints que tienen
    // [ApiExplorerSettings(GroupName = "Tasks"/"Users")] porque no coinciden con "v1".
    options.DocInclusionPredicate((_, _) => true);

    // Usa el GroupName de [ApiExplorerSettings] como nombre de sección en Swagger UI.
    // Muestra "Tasks" y "Users" en lugar de "Tareas" y "Usuarios".
    options.TagActionsBy(api =>
    {
        var groupName = api.GroupName;
        return groupName is not null
            ? [groupName]
            : [api.ActionDescriptor.RouteValues["controller"]!];
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

// ── Redirección raíz → Swagger ────────────────────────────────────────────────
// Redirige la URL raíz "/" a "/swagger" para facilitar el acceso a la documentación.
app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// Configure the HTTP request pipeline.
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
