using Cassandra;
using Cassandra.Mapping;
using HoloRed.Infrastructure.Cassandra;
using HoloRed.Infrastructure.Neo4j;
using HoloRed.Infrastructure.Repositories;
using HoloRed.Domain.Interfaces;
using HoloRed.Service;
using HoloRed.Services;
using Neo4j.Driver;
using StackExchange.Redis;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN DE SWAGGER ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HoloRed API - Sistema Central de la Nueva República",
        Version = "v1",
        Description = "Sistemas Políglotas (SQL Server, Cassandra, Neo4j, Redis)"
    });
});

// --- 2. REGISTRO DE REPOSITORIOS Y SERVICIOS (OBLIGATORIO) ---
// Registramos los tipos siempre al principio para que la App no explote al validar dependencias
builder.Services.AddScoped<CassandraTelemetriaRepository>();
builder.Services.AddScoped<IEspionajeRepository, Neo4jEspionajeRepository>();
builder.Services.AddScoped<IRadarRepository, RedisRadarRepository>();

builder.Services.AddScoped<IInteligenciaService, InteligenciaService>();
builder.Services.AddSingleton<AtraqueService>();

// --- 3. CONEXIÓN A MOTORES DE DATOS ---

// A. CASSANDRA (Telemetría)
try
{
    var cluster = global::Cassandra.Cluster.Builder()
        .AddContactPoint("127.0.0.1")
        .WithPort(9042) // Añadido el puerto
        .WithCredentials("admin", "RepublicBattle_2026!") // Añadidas las credenciales
        .Build();

    // Intentamos conectar
    var session = cluster.Connect();

    // Registramos la sesión real
    builder.Services.AddSingleton<global::Cassandra.ISession>(session);

    global::Cassandra.Mapping.MappingConfiguration.Global.Define<CassandraMappingConfig>();
    Console.WriteLine(">>> [OK] Cassandra conectada.");
}
catch (Exception ex)
{
    Console.WriteLine($">>> [AVISO] Cassandra no disponible: {ex.Message}");

    // REGISTRO DE SEGURIDAD: Inyectamos un valor nulo para que el constructor del Repositorio 
    // no haga que la aplicación se cierre al arrancar.
    builder.Services.AddSingleton<global::Cassandra.ISession>(sp => null!);
}

// B. NEO4J (Espionaje)
try
{
    var neo4jDriver = global::Neo4j.Driver.GraphDatabase.Driver(
    "bolt://localhost:7687",
    global::Neo4j.Driver.AuthTokens.Basic("neo4j", "RepublicSpies_2026!")
);
    builder.Services.AddSingleton<global::Neo4j.Driver.IDriver>(neo4jDriver);
    Console.WriteLine(">>> [OK] Neo4j conectado.");
}
catch (Exception ex)
{
    Console.WriteLine($">>> [AVISO] Neo4j no disponible: {ex.Message}");
    builder.Services.AddSingleton<global::Neo4j.Driver.IDriver>(sp => null!);
}

// C. REDIS (Radar - Álvaro)
var redisConnectionString = "localhost:6379,password=RepublicRadar_2024!,abortConnect=false";
try
{
    var connection = ConnectionMultiplexer.Connect(redisConnectionString);
    builder.Services.AddSingleton<IConnectionMultiplexer>(connection);
    Console.WriteLine(">>> [OK] Redis conectado.");
}
catch (Exception ex)
{
    Console.WriteLine($">>> [AVISO] Redis no disponible: {ex.Message}");
}

// --- 4. CONSTRUCCIÓN DE LA APP ---
var app = builder.Build();

// --- 5. PIPELINE DE EJECUCIÓN (MIDDLEWARE) ---
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
app.MapControllers();

app.Run();