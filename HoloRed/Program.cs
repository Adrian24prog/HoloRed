using Cassandra;
using Cassandra.Mapping;
using HoloRed.Infrastructure.Cassandra;
using HoloRed.Infrastructure.Neo4j;
using HoloRed.Domain.Interfaces;
using HoloRed.Service; // <-- Nueva capa de lógica de negocio
using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICIOS BASE DE LA API ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 2. CONFIGURACIÓN DE CASSANDRA (TELEMETRÍA) ---
try
{
    var cluster = global::Cassandra.Cluster.Builder()
        .AddContactPoint("127.0.0.1")
        .Build();

    var session = cluster.Connect();

    // Activamos los mapeos de la infraestructura de Cassandra
    global::Cassandra.Mapping.MappingConfiguration.Global.Define<CassandraMappingConfig>();

    builder.Services.AddSingleton<global::Cassandra.ISession>(session);
    builder.Services.AddScoped<CassandraTelemetriaRepository>();

    Console.WriteLine(">>> [OK] Conexión con la HoloRed (Cassandra) establecida.");
}
catch (Exception ex)
{
    Console.WriteLine($">>> [ERROR] No se pudo conectar a Cassandra: {ex.Message}");
}

// --- 3. CONFIGURACIÓN DE NEO4J (RED DE ESPIONAJE) ---
try
{
    var neo4jDriver = global::Neo4j.Driver.GraphDatabase.Driver(
        "bolt://localhost:7687",
        global::Neo4j.Driver.AuthTokens.Basic("neo4j", "password")
    );

    builder.Services.AddSingleton<global::Neo4j.Driver.IDriver>(neo4jDriver);
    builder.Services.AddScoped<IEspionajeRepository, Neo4jEspionajeRepository>();

    Console.WriteLine(">>> [OK] Conexión con la Red de Espionaje (Neo4j) establecida.");
}
catch (Exception ex)
{
    Console.WriteLine($">>> [ERROR] No se pudo conectar a Neo4j: {ex.Message}");
}

// --- 4. REGISTRO DE LA CAPA DE SERVICIO (LOGICA DE NEGOCIO) ---
// Este es el "cerebro" que coordina tus dos bases de datos (Cassandra y Neo4j)
builder.Services.AddScoped<IInteligenciaService, InteligenciaService>();

var app = builder.Build();

// --- 5. CONFIGURACIÓN DEL PIPELINE (MIDDLEWARE) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();