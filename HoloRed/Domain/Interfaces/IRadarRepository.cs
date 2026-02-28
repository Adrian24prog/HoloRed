namespace HoloRed.Domain.Interfaces;



/// <summary>
/// Interfaz para la gestión del Radar en tiempo real.
/// Requisito: Tiempos de respuesta de submilisegundos usando Clave-Valor.
/// </summary>
/// <author>Álvaro Naranjo</author>
/// <date>28/02/2026</date>
public interface IRadarRepository
{
    /// <summary>
    /// Actualiza el estado de la nave y renueva su TTL de 10 minutos.
    /// Si la nave no emite señal en ese tiempo, desaparecerá del radar.
    /// </summary>
    /// <param name="codigoNave">Identificador único de la nave.</param>
    /// <param name="estado">Estado actual (patrulla, hiperespacio o combate).</param>
    Task ActualizarBalizaAsync(string codigoNave, string estado);


    /// <summary>
    /// Existes the nave asynchronous.
    /// </summary>
    /// <param name="codigoNave">The codigo nave.</param>
    /// <returns></returns>
    Task<bool> ExisteNaveAsync(string codigoNave);
}



