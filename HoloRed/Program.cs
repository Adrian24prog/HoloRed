using Cassandra;
using Cassandra.Mapping;
using HoloRed.Domain.Interfaces;
using HoloRed.Infrastructure.Cassandra;
using HoloRed.Infrastructure.Neo4j;
using HoloRed.Infrastructure.Repositories;
using HoloRed.Service;
using HoloRed.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Neo4j.Driver;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        //Captura de errores de validación de modelo para mejorar la respuesta al cliente
        options.InvalidModelStateResponseFactory = context =>
        {
            return new BadRequestObjectResult(new
            {
                error = "Datos de telemetría corruptos",
                mensaje = "El formato del impacto no es válido. Revise los tipos introducidos.",
                detalles = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });
        };
    });

// Configuración 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HoloRed API", Version = "v1" });
});



// --- 2. SERVICIOS Y REPOSITORIOS ---
builder.Services.AddScoped<CassandraTelemetriaRepository>();
builder.Services.AddScoped<IEspionajeRepository, Neo4jEspionajeRepository>();
builder.Services.AddScoped<IRadarRepository, RedisRadarRepository>();
builder.Services.AddScoped<IInteligenciaService, InteligenciaService>();
builder.Services.AddSingleton<AtraqueService>();

// --- 3. CONEXIONES ---

// A. CASSANDRA
try
{
    // ACTIVACIÓN DEL MAPPER (Crítico para que el JSON no salga [])
    global::Cassandra.Mapping.MappingConfiguration.Global.Define<CassandraMappingConfig>();

    var cluster = global::Cassandra.Cluster.Builder()
        .AddContactPoint("127.0.0.1")
        .WithPort(9042)
        .WithCredentials("admin", "RepublicBattle_2026!")
        .Build();

    var session = cluster.Connect("holored");
    builder.Services.AddSingleton<global::Cassandra.ISession>(session);
    Console.WriteLine(">>> [OK] Cassandra conectada y mapeo activado.");
}
catch (Exception ex)
{
    Console.WriteLine($">>> [ERROR] Cassandra offline: {ex.Message}");
    builder.Services.AddSingleton<global::Cassandra.ISession>(sp => null!);
}

// B. NEO4J
try
{
    var neo4jDriver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "RepublicSpies_2026!"));
    builder.Services.AddSingleton<IDriver>(neo4jDriver);
    Console.WriteLine(">>> [OK] Neo4j conectado.");
}
catch { builder.Services.AddSingleton<IDriver>(sp => null!); }

// C. REDIS
try
{
    var redis = ConnectionMultiplexer.Connect("localhost:6379,password=RepublicRadar_2024!,abortConnect=false");
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    Console.WriteLine(">>> [OK] Redis conectado.");
}
catch { }

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapControllers();
app.Run();