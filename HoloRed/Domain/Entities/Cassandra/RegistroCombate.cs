using System;
using Cassandra;
using Cassandra.Mapping.Attributes; // Librería crítica para el mapeo objeto-relacional (ORM)

namespace HoloRed.Domain.Entities.Cassandra;

/// <summary>
/// Entidad de Dominio que representa un registro de telemetría de combate.
/// Mapea directamente con la tabla 'telemetria_combate' en el Keyspace de Cassandra.
/// </summary>
/// <remarks>
/// Autor: Adrian Dondarza
/// Esta clase utiliza Atributos de Mapeo para forzar la compatibilidad con nombres
/// de columna en formato 'snake_case' (minúsculas y guiones bajos).
/// </remarks>
[Table("telemetria_combate")]
public class RegistroCombate
{
    /// <summary>
    /// Identificador del sector estelar.
    /// Definido como PARTITION KEY para agrupar las batallas físicamente por ubicación.
    /// </summary>
    [PartitionKey]
    [Column("sector_id")]
    public string SectorId { get; set; } = string.Empty;

    /// <summary>
    /// Fecha del suceso (Formato Cassandra LocalDate: 4 bytes).
    /// Definida como CLUSTERING KEY (0) para ordenar los eventos cronológicamente por día.
    /// </summary>
    [ClusteringKey(0)]
    [Column("fecha")]
    public LocalDate Fecha { get; set; }

    /// <summary>
    /// Marca de tiempo precisa (incluye nanosegundos).
    /// Definida como CLUSTERING KEY (1) para desempatar eventos ocurridos el mismo día.
    /// </summary>
    [ClusteringKey(1)]
    [Column("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Nombre o identificador de la nave que realiza la agresión.
    /// </summary>
    [Column("nave_atacante")]
    public string NaveAtacante { get; set; } = string.Empty;

    /// <summary>
    /// Nombre o identificador de la nave que recibe el impacto.
    /// </summary>
    [Column("nave_objetivo")]
    public string NaveObjetivo { get; set; } = string.Empty;

    /// <summary>
    /// Valor entero que representa el daño mitigado o recibido en los escudos.
    /// Mapeado a 'dano_escudos' para evitar conflictos con caracteres especiales (ñ).
    /// </summary>
    [Column("dano_escudos")]
    public int DanoEscudos { get; set; }
}