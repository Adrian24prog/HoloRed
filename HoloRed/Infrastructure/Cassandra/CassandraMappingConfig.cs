using Cassandra.Mapping;
using HoloRed.Domain.Entities.Cassandra;

namespace HoloRed.Infrastructure.Cassandra;

public class CassandraMappingConfig : global::Cassandra.Mapping.Mappings
{
    public CassandraMappingConfig()
    {
        var mapConfig = For<RegistroCombate>()
            .TableName("telemetria_combate");

        mapConfig.PartitionKey(x => x.SectorId);

        mapConfig.ClusteringKey(x => x.Fecha)
                 .ClusteringKey(x => x.Timestamp);

        mapConfig.Column(x => x.SectorId, cm => cm.WithName("sector_id"));
        mapConfig.Column(x => x.Fecha, cm => cm.WithName("fecha"));
        mapConfig.Column(x => x.Timestamp, cm => cm.WithName("timestamp"));
        mapConfig.Column(x => x.NaveAtacante, cm => cm.WithName("nave_atacante"));
        mapConfig.Column(x => x.NaveObjetivo, cm => cm.WithName("nave_objetivo"));
        mapConfig.Column(x => x.DanioEscudos, cm => cm.WithName("danio_escudos"));
    }
}