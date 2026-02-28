using HoloRed.Domain.Interfaces;
using HoloRed.Infrastructure.Repositories;
using HoloRed.Services;
using StackExchange.Redis;
using Microsoft.OpenApi.Models; // <--- ¡YA NO DEBERÍA ESTAR EN ROJO!

var builder = WebApplication.CreateBuilder(args);

// --- SECCIÓN DE SERVICIOS ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    // Usamos OpenApiInfo de la librería .Models
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HoloRed API - Nueva República",
        Version = "v1"
    });
});

// CONFIGURACIÓN DE REDIS (Álvaro)
var redisConnectionString = "localhost:6379,password=RepublicRadar_2024!,abortConnect=false";
try
{
    var connection = ConnectionMultiplexer.Connect(redisConnectionString);
    builder.Services.AddSingleton<IConnectionMultiplexer>(connection);
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Error conectando a Redis: {ex.Message}");
}

// REGISTRO DE DEPENDENCIAS
builder.Services.AddScoped<IRadarRepository, RedisRadarRepository>();
builder.Services.AddSingleton<AtraqueService>();

var app = builder.Build();

// --- PIPELINE DE EJECUCIÓN ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HoloRed v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // <-- El error de Reflection se irá tras limpiar carpetas

app.Run();