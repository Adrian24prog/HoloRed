using System;
using Cassandra;
using Cassandra.Mapping.Attributes; // Librería crítica para el mapeo objeto-relacional (ORM)

namespace HoloRed.Domain.Entities.Cassandra;

/// <summary>
/// Entidad de Dominio que representa un registro de telemetría de combate galáctico.
/// Esta clase orquesta el mapeo directo con la familia de columnas 'telemetria_combate'.
/// </summary>
/// <remarks>
/// <author>Adrian Dondarza</author>
/// <date>02/03/2026</date>
/// </remarks>
[Table("telemetria_combate")]
public class RegistroCombate
{
    /// <summary>
    /// Identificador del sector estelar donde ocurre el evento.
    /// Definido como PARTITION KEY para optimizar la distribución física de los datos en el clúster.
    /// </summary>
    [PartitionKey]
    [Column("sector_id")]
    public string SectorId { get; set; } = string.Empty;

    /// <summary>
    /// Fecha del suceso astronómico (Formato Cassandra LocalDate).
    /// Definida como CLUSTERING KEY (0) para garantizar el ordenamiento cronológico nativo.
    /// </summary>
    [ClusteringKey(0)]
    [Column("fecha")]
    public LocalDate Fecha { get; set; }

    /// <summary>
    /// Marca de tiempo precisa con resolución de milisegundos.
    /// Definida como CLUSTERING KEY (1) para asegurar la unicidad y el orden dentro de una misma fecha.
    /// </summary>
    [ClusteringKey(1)]
    [Column("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Identificador o firma de la nave agresora detectada por el radar.
    /// </summary>
    [Column("nave_atacante")]
    public string NaveAtacante { get; set; } = string.Empty;

    /// <summary>
    /// Identificador o firma de la nave que recibe el impacto bláster.
    /// </summary>
    [Column("nave_objetivo")]
    public string NaveObjetivo { get; set; } = string.Empty;

    /// <summary>
    /// Magnitud del impacto absorbido o mitigado por los sistemas de defensa.
    /// Mapeado a 'danio_escudos' para cumplir con la compatibilidad de caracteres del driver.
    /// </summary>
    [Column("danio_escudos")]
    public int DanoEscudos { get; set; }
}