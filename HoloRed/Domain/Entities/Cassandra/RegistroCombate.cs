namespace HoloRed.Domain.Entities.Cassandra;

/// <summary>
/// Entidad que representa la tabla de telemetría en Cassandra.
/// Diseñada para optimizar la lectura por Sector y Fecha.
/// </summary>
/// <remarks>
/// Autor: Adrian Dondarza
/// </remarks>
public class RegistroCombate
{
    /// <summary>
    /// Partition Key: Determina en qué nodo se guardan los datos.
    /// </summary>
    public string SectorId { get; set; }

    /// <summary>
    /// Clustering Key (1): Permite filtrar por día específico.
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Clustering Key (2): Mantiene el orden cronológico dentro del día.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    public string NaveAtacante { get; set; }
    public string NaveObjetivo { get; set; }
    public int DañoEscudos { get; set; }
}
