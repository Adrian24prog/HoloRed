using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace HoloRed.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de Radar utilizando Redis como almacenamiento
/// en memoria basado en estructura Clave-Valor.
/// Permite actualizar la baliza de una nave con tiempos de respuesta submilisegundos.
/// </summary>
/// <author>Álvaro Naranjo</author>
/// <date>28/02/2026</date>
public class RedisRadarRepository
{
    /// <summary>
    /// Referencia a la base de datos de Redis
    /// </summary>
    private readonly IDatabase _db;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio de Radar utilizando
    /// una conexión activa a Redis.
    /// </summary>
    /// <param name="redis">Multiplexor de conexión a Redis.</param>
    public RedisRadarRepository(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    /// <summary>
    /// Actualiza el estado de la nave en Redis y renueva su TTL de 10 minutos.
    /// Si la nave no emite señal dentro de ese período, desaparecerá automáticamente del radar.
    /// </summary>
    /// <param name="codigoNave">Identificador único de la nave.</param>
    /// <param name="estado">Estado actual (patrulla, hiperespacio o combate).</param>
    /// <returns>Tarea asíncrona que representa la operación de actualización.</returns>
    public async Task ActualizarBalizaAsync(string codigoNave, string estado)
    {
        // TTL de 10 minutos para que la nave desaparezca del radar si no emite señal.
        var expiracion = TimeSpan.FromMinutes(10);

        // Clave: nave:{codigo} | Valor: estado
        await _db.StringSetAsync($"nave:{codigoNave}", estado, expiracion);
    }
}