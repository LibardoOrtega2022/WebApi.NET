using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Reflection;
using WebAPIUser.Data;
using WebAPIUser.Middleware;
using WebAPIUser.Models;
using WebAPIUser.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DbUserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("connectionDB")));

// Registrar servicios
builder.Services.AddScoped<IMapperService, MapperService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ITareaService, TareaService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
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

    // ✅ Le dice a Swagger dónde está el archivo XML generado
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Usar middleware de manejo de excepciones
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI();
    app.UseSwagger();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Inicializar datos de prueba (opcional)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbUserContext>();

    // Asegurar que las migraciones están aplicadas
    await db.Database.MigrateAsync();

    // Llenar datos de prueba si es necesario
    await DataSeeder.InicializarDatosAsync(db);
}



app.Run();
