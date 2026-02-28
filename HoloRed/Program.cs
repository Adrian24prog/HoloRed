using Cassandra;
using Cassandra.Mapping;
using HoloRed.Infrastructure.Cassandra;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICIOS BASE DE LA API ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Usamos Swagger para probar los endpoints fácilmente

// --- 2. CONFIGURACIÓN DE PERSISTENCIA (CASSANDRA) ---
try
{
    // Definimos la conexión al nodo de Cassandra (Docker)
    var cluster = Cluster.Builder()
        .AddContactPoint("127.0.0.1")
        .Build();

    // Creamos la sesión activa
    var session = cluster.Connect();

    // REGLA DE ORO: Activamos los mapeos de Adrián Dondarza 
    // Esto vincula tus clases de C# con las tablas de la base de datos
    MappingConfiguration.Global.Define<CassandraMappingConfig>();

    // Inyectamos la sesión como Singleton (una sola conexión para toda la app)
    builder.Services.AddSingleton<global::Cassandra.ISession>(session);

    // Inyectamos el repositorio para que los Controllers puedan usarlo
    builder.Services.AddScoped<CassandraTelemetriaRepository>();

    Console.WriteLine(">>> Conexión con la HoloRed (Cassandra) establecida con éxito.");
}
catch (Exception ex)
{
    // Si Cassandra no está levantado en Docker, mostramos el aviso pero la API no se detiene
    Console.WriteLine($"[ATENCIÓN] No se pudo conectar a Cassandra: {ex.Message}");
}

var app = builder.Build();

// --- 3. CONFIGURACIÓN DEL PIPELINE (MIDDLEWARE) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();