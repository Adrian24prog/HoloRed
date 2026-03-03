using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using HoloRed.Domain.Entities.Cassandra;

namespace HoloRed.Infrastructure.Cassandra;

public class CassandraTelemetriaRepository
{
    private readonly global::Cassandra.ISession _session;
    private readonly global::Cassandra.Mapping.IMapper _mapper;

    public CassandraTelemetriaRepository(global::Cassandra.ISession session)
    {
        _session = session;

        // Configuramos el mapeo asegurando que apunte al Keyspace correcto
        var mappingConfig = new global::Cassandra.Mapping.MappingConfiguration();
        mappingConfig.Define(new global::Cassandra.Mapping.Map<RegistroCombate>()
            .TableName("impactos")      // Nombre real de la tabla en Cassandra
            .KeyspaceName("holored")    // Especificamos el Keyspace para evitar el error 503
            .PartitionKey(u => u.SectorId, u => u.Fecha)
            .Column(u => u.DanoEscudos, cm => cm.WithName("danoescudos"))); 

        _mapper = new global::Cassandra.Mapping.Mapper(session, mappingConfig);
    }

    public async Task RegistrarImpactoAsync(RegistroCombate impacto)
    {
        // El mapper --> a holored.impactos
        await _mapper.InsertAsync(impacto);
    }

    public async Task<IEnumerable<RegistroCombate>> ObtenerHistorialPorSectorAsync(string sectorId, DateTime fecha)
    {
        var cassandraFecha = new global::Cassandra.LocalDate(fecha.Year, fecha.Month, fecha.Day);

        // Actualizamos la consulta para usar el nombre de tabla correcto: 'impactos'
        var query = global::Cassandra.Mapping.Cql.New(
            "SELECT * FROM holored.impactos WHERE SectorId = ? AND Fecha = ?",
            sectorId,
            cassandraFecha);

        return await _mapper.FetchAsync<RegistroCombate>(query);
    }
}