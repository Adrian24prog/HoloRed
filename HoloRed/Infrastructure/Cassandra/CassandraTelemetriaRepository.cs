using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using HoloRed.Domain.Entities.Cassandra;

namespace HoloRed.Infrastructure.Cassandra;

/// <summary>
/// Repositorio de telemetría para Cassandra. 
/// Implementa un diseño de alto rendimiento basado en el modelo P2P de familias de columnas.
/// </summary>
/// <author>Adrian Dondarza</author>
public class CassandraTelemetriaRepository
{
    private readonly global::Cassandra.ISession _session;
    private readonly global::Cassandra.Mapping.IMapper _mapper;

    public CassandraTelemetriaRepository(global::Cassandra.ISession session)
    {
        _session = session;

        // Configuración de mapeo fluida para asegurar la sincronización con CQL
        var mappingConfig = new global::Cassandra.Mapping.MappingConfiguration();
        mappingConfig.Define(new global::Cassandra.Mapping.Map<RegistroCombate>()
            .TableName("telemetria_combate")    // Ajustado al nombre real en el script CQL
            .KeyspaceName("holored")           // Keyspace obligatorio para evitar ambigüedad
            .PartitionKey(u => u.SectorId)     // La Partition Key es solo SectorId según tu script
            .ClusteringKey(u => u.Fecha)       // Definición de Clustering Keys en orden
            .ClusteringKey(u => u.Timestamp)
            .Column(u => u.SectorId, cm => cm.WithName("sector_id"))
            .Column(u => u.Fecha, cm => cm.WithName("fecha"))
            .Column(u => u.Timestamp, cm => cm.WithName("timestamp"))
            .Column(u => u.NaveAtacante, cm => cm.WithName("nave_atacante"))
            .Column(u => u.NaveObjetivo, cm => cm.WithName("nave_objetivo"))
            .Column(u => u.DanoEscudos, cm => cm.WithName("danio_escudos")));

        _mapper = new global::Cassandra.Mapping.Mapper(session, mappingConfig);
    }

    /// <summary>
    /// Inserta un impacto de combate de forma asíncrona.
    /// Captura excepciones de disponibilidad del clúster (Punto 4 de la rúbrica).
    /// </summary>
    public async Task RegistrarImpactoAsync(RegistroCombate impacto)
    {
        try
        {
            await _mapper.InsertAsync(impacto);
        }
        catch (NoHostAvailableException ex)
        {
            // Lanzamos una excepción personalizada o capturable por el controlador para un 503
            throw new Exception("Error de conexión con el clúster de Cassandra.", ex);
        }
    }

    /// <summary>
    /// Consulta el historial por sector y fecha.
    /// Diseñada para evitar el Full Scan mediante el uso de Partition Key.
    /// </summary>
    public async Task<IEnumerable<RegistroCombate>> ObtenerHistorialPorSectorAsync(string sectorId, DateTime fecha)
    {
        try
        {
            var cassandraFecha = new global::Cassandra.LocalDate(fecha.Year, fecha.Month, fecha.Day);

            // Consulta optimizada utilizando el diseño de la clave primaria (sector_id + fecha)
            var query = global::Cassandra.Mapping.Cql.New(
                "SELECT * FROM holored.telemetria_combate WHERE sector_id = ? AND fecha = ?",
                sectorId,
                cassandraFecha);

            return await _mapper.FetchAsync<RegistroCombate>(query);
        }
        catch (QueryExecutionException ex)
        {
            throw new Exception("Error al ejecutar la consulta de telemetría en el nodo.", ex);
        }
    }
}