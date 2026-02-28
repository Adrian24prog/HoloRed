using Cassandra.Mapping;
using HoloRed.Domain.Entities;

namespace HoloRed.Infrastructure.Cassandra;

/// <summary>
/// Configuración del mapeo entre los objetos C# y las tablas de Cassandra.
/// Define la estructura de la clave primaria para optimizar búsquedas.
/// </summary>
/// <remarks>
/// Autor: Adrian Dondarza
/// </remarks>
public class CassandraMappingConfig : global::Cassandra.Mapping.Mappings
{
    public CassandraMappingConfig()
    {
        // 1. Iniciamos el mapeo para la entidad RegistroCombate
        var mapConfig = For<global::HoloRed.Domain.Entities.RegistroCombate>()
            .TableName("telemetria_combate");

        // 2. Definimos la Partition Key (Clave de Partición)
        // Esto agrupa los datos físicamente por Sector en el clúster.
        mapConfig.PartitionKey(x => x.SectorId);

        // 3. Definimos las Clustering Keys (Claves de Agrupamiento) una a una
        // Esto permite filtrar por fecha y mantener el orden por timestamp sin errores.
        mapConfig.ClusteringKey(x => x.Fecha);
        mapConfig.ClusteringKey(x => x.Timestamp);

        // 4. Mapeo explícito de columnas (C# Property -> Cassandra Column)
        mapConfig.Column(x => x.SectorId, cm => cm.WithName("sector_id"));
        mapConfig.Column(x => x.Fecha, cm => cm.WithName("fecha"));
        mapConfig.Column(x => x.Timestamp, cm => cm.WithName("timestamp"));
        mapConfig.Column(x => x.NaveAtacante, cm => cm.WithName("nave_atacante"));
        mapConfig.Column(x => x.NaveObjetivo, cm => cm.WithName("nave_objetivo"));
        mapConfig.Column(x => x.DañoEscudos, cm => cm.WithName("danio_escudos"));
    }
}