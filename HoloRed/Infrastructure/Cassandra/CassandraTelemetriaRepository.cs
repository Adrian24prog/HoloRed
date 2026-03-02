using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cassandra; // Librería base
using Cassandra.Mapping; // Librería de mapeo
using HoloRed.Domain.Entities.Cassandra;

namespace HoloRed.Infrastructure.Cassandra;

public class CassandraTelemetriaRepository
{
    // Usamos el prefijo completo para evitar errores de "ambigüedad"
    private readonly global::Cassandra.ISession _session;
    private readonly global::Cassandra.Mapping.IMapper _mapper;

    public CassandraTelemetriaRepository(global::Cassandra.ISession session)
    {
        _session = session;

        // 1. Configuramos el mapeo localmente
        var mappingConfig = new global::Cassandra.Mapping.MappingConfiguration();
        mappingConfig.Define<HoloRed.Infrastructure.Cassandra.CassandraMappingConfig>();

        // 2. Inicializamos el Mapper con la sesión y la configuración
        _mapper = new global::Cassandra.Mapping.Mapper(session, mappingConfig);
    }

    public async Task RegistrarImpactoAsync(RegistroCombate impacto)
    {
        await _mapper.InsertAsync(impacto);
    }

    public async Task<IEnumerable<RegistroCombate>> ObtenerHistorialPorSectorAsync(string sectorId, DateTime fecha)
    {
        // LocalDate es un tipo propio de Cassandra (4 bytes para fecha)
        var cassandraFecha = new global::Cassandra.LocalDate(fecha.Year, fecha.Month, fecha.Day);

        // Usamos Cql.New para que el Mapper sepa cómo traducir los nombres de las columnas
        var query = global::Cassandra.Mapping.Cql.New(
            "SELECT * FROM telemetria_combate WHERE sector_id = ? AND fecha = ?",
            sectorId,
            cassandraFecha);

        return await _mapper.FetchAsync<RegistroCombate>(query);
    }
}