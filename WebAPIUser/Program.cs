using Microsoft.EntityFrameworkCore;
using WebAPIUser.Models;
using WebAPIUser.Services;
using WebAPIUser.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DbUserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("connectionDB")));

// Registrar servicios
builder.Services.AddScoped<IMapperService, MapperService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

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

app.Run();
