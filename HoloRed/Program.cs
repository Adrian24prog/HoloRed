ï»¿using HoloRed.Domain.Interfaces;
using HoloRed.Infrastructure.Repositories;
using HoloRed.Services;
using StackExchange.Redis;
using Microsoft.OpenApi.Models; // <--- Â¡YA NO DEBERÃA ESTAR EN ROJO!

var builder = WebApplication.CreateBuilder(args);

// --- SECCIÃN DE SERVICIOS ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    // Usamos OpenApiInfo de la librerÃ­a .Models
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HoloRed API - Nueva RepÃºblica",
        Version = "v1"
    });
});

// CONFIGURACIÃN DE REDIS (Ãlvaro)
var redisConnectionString = "localhost:6379,password=RepublicRadar_2024!,abortConnect=false";
try
{
    var connection = ConnectionMultiplexer.Connect(redisConnectionString);
    builder.Services.AddSingleton<IConnectionMultiplexer>(connection);
}
catch (Exception ex)
{
    Console.WriteLine($"â ï¸ Error conectando a Redis: {ex.Message}");
}

// REGISTRO DE DEPENDENCIAS
builder.Services.AddScoped<IRadarRepository, RedisRadarRepository>();
builder.Services.AddSingleton<AtraqueService>();

var app = builder.Build();

// --- PIPELINE DE EJECUCIÃN ---
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
app.MapControllers(); // <-- El error de Reflection se irÃ¡ tras limpiar carpetas

app.Run();