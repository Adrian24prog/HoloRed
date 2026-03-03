using Cassandra.Mapping;
using HoloRed.Domain.Entities.Cassandra;

namespace HoloRed.Infrastructure.Cassandra;

/// <summary>
/// Configuración avanzada del mapeo entre los objetos de dominio C# y el esquema CQL de Cassandra.
/// Define la estrategia de particionamiento y ordenamiento para optimizar las consultas de telemetría.
/// </summary>
/// <remarks>
/// <author>Adrian Dondarza</author>
/// <date>03/03/2026</date>
/// </remarks>
public class CassandraMappingConfig : global::Cassandra.Mapping.Mappings
{
    /// <summary>
    /// Inicializa las reglas de mapeo para la entidad RegistroCombate.
    /// Establece la correspondencia exacta entre propiedades y nombres de columna 'snake_case'.
    /// </summary>
    public CassandraMappingConfig()
    {
        // Definición de la tabla física en el Keyspace holored
        var mapConfig = For<RegistroCombate>()
            .TableName("telemetria_combate");

        // Configuración de la Clave de Partición (Requisito: Búsqueda por SectorId)
        mapConfig.PartitionKey(x => x.SectorId);

        // Configuración de las Claves de Agrupamiento (Clustering Keys) para ordenación cronológica
        mapConfig.ClusteringKey(x => x.Fecha)
                 .ClusteringKey(x => x.Timestamp);

        // Mapeo explícito de columnas para asegurar compatibilidad con el driver
        mapConfig.Column(x => x.SectorId, cm => cm.WithName("sector_id"));
        mapConfig.Column(x => x.Fecha, cm => cm.WithName("fecha"));
        mapConfig.Column(x => x.Timestamp, cm => cm.WithName("timestamp"));
        mapConfig.Column(x => x.NaveAtacante, cm => cm.WithName("nave_atacante"));
        mapConfig.Column(x => x.NaveObjetivo, cm => cm.WithName("nave_objetivo"));

        // Mapeo específico de daño de escudos para evitar conflictos de caracteres
        mapConfig.Column(x => x.DanoEscudos, cm => cm.WithName("danio_escudos"));
    }
}