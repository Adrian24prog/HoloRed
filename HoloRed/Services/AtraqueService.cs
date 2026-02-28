using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HoloRed.Services;

/// <summary>
/// Servicio encargado de la gestión de atraques en bahías espaciales.
/// Implementa control de concurrencia mediante SemaphoreSlim para
/// evitar colisiones cuando múltiples naves intentan atracar simultáneamente.
/// </summary>
/// <author>Álvaro Naranjo</author>
/// <date>28/02/2026</date>
public class AtraqueService
{
    /// <summary>
    /// Semáforo que garantiza acceso exclusivo a la sección crítica
    /// donde se valida y registra la ocupación de una bahía.
    /// </summary>
    private static readonly SemaphoreSlim _semaforo = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Conjunto en memoria que simula las bahías actualmente ocupadas.
    /// </summary>
    private static readonly HashSet<int> _bahiasOcupadas = new HashSet<int>();

    /// <summary>
    /// Intenta realizar el atraque de una nave en una bahía específica.
    /// Aplica control de concurrencia para evitar que dos naves
    /// ocupen la misma bahía simultáneamente.
    /// </summary>
    /// <param name="numeroBahia">Número identificador de la bahía espacial.</param>
    /// <param name="codigoNave">Identificador único de la nave solicitante.</param>
    /// <returns>
    /// True si el atraque se realiza con éxito.  
    /// False si la bahía ya se encuentra ocupada.
    /// </returns>
    public async Task<bool> IntentarAtraqueAsync(int numeroBahia, string codigoNave)
    {
        // Esperamos nuestro turno para entrar a la zona crítica
        await _semaforo.WaitAsync();

        try
        {
            // Control hilo a hilo para evitar colisiones en la bahía
            if (_bahiasOcupadas.Contains(numeroBahia))
            {
                return false; // Bahía ocupada, evitamos la colisión
            }

            // Simulación del proceso de atraque
            _bahiasOcupadas.Add(numeroBahia);
            await Task.Delay(500); // Tiempo que tarda la maniobra

            return true;
        }
        finally
        {
            // Liberación del semáforo garantizada incluso ante excepción
            _semaforo.Release();
        }
    }
}