using HoloRed.Domain.Interfaces;
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
public class RedisRadarRepository : IRadarRepository
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

    /// <summary>
    /// Existes the nave asynchronous.
    /// </summary>
    /// <param name="codigoNave">The codigo nave.</param>
    /// <returns></returns>
     public async Task<bool> ExisteNaveAsync(string codigoNave)
    {
        // Verifica si la clave "nave:nombre" existe en Redis
        return await _db.KeyExistsAsync($"nave:{codigoNave}");
    }

    /// <summary>
    /// Intenta asignar una nave a una bahía de atraque específica.
    /// Utiliza bloqueos distribuidos (Locks) para garantizar que dos naves
    /// no ocupen el mismo lugar simultáneamente (Thread-Safe).
    /// </summary>
    public async Task<bool> AsignarAtraqueAsync(int numeroBahia, string codigoNave)
    {
        string lockKey = $"lock:bahia:{numeroBahia}";
        string resourceKey = $"bahia:{numeroBahia}";

        // Intentamos adquirir un bloqueo de 5 segundos para realizar la operación
        if (await _db.LockTakeAsync(lockKey, codigoNave, TimeSpan.FromSeconds(5)))
        {
            try
            {
                // Verificamos si la bahía ya está ocupada por otra nave
                if (await _db.KeyExistsAsync(resourceKey))
                {
                    return false; // Bahía ya ocupada
                }

                // Asignamos la nave a la bahía (Sin TTL, el atraque es permanente hasta salida)
                return await _db.StringSetAsync(resourceKey, codigoNave);
            }
            finally
            {
                // Liberamos el bloqueo pase lo que pase
                await _db.LockReleaseAsync(lockKey, codigoNave);
            }
        }

        return false; 
    }
}