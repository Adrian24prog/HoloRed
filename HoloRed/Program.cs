var builder = WebApplication.CreateBuilder(args);

// --- SECCIÓN DE SERVICIOS ---

builder.Services.AddControllers();
builder.Services.AddOpenApi();

//  CONFIGURACIÓN DE REDIS Alvaro
// Usamos la contraseña que definimos en el docker-compose: RepublicRadar_2024!
var redisConnectionString = "localhost:6379,password=RepublicRadar_2024!";

// Registramos el multiplexor como Singleton (una sola conexión para toda la App)
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(
    StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString));

// Registramos tu repositorio para que el Controller pueda pedirlo por el constructor
builder.Services.AddScoped<HoloRed.Infrastructure.Repositories.RedisRadarRepository>();
builder.Services.AddSingleton<HoloRed.Services.AtraqueService>();
// --------------------------------------------------------

var app = builder.Build(); 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();