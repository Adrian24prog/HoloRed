using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using HoloRed.Domain.Entities.Cassandra;

namespace HoloRed.Infrastructure.Cassandra;

/// <summary>
/// Repositorio de infraestructura encargado de la persistencia en Apache Cassandra.
/// Implementa un modelo de almacenamiento de familias de columnas optimizado para 
/// escrituras masivas (append-only) y consultas de baja latencia.
/// </summary>
/// <remarks>
/// <author>Adrian Dondarza</author>
/// <date>03/03/2026</date>
/// </remarks>
public class CassandraTelemetriaRepository
{
    private readonly global::Cassandra.ISession _session;
    private readonly global::Cassandra.Mapping.IMapper _mapper;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio configurando el mapeo fluido 
    /// para sincronizar las entidades de C# con el esquema de base de datos CQL.
    /// </summary>
    /// <param name="session">Sesión activa del clúster de Cassandra.</param>
    public CassandraTelemetriaRepository(global::Cassandra.ISession session)
    {
        _session = session;

        // Configuración de mapeo fluida para asegurar la sincronización con el motor P2P
        var mappingConfig = new global::Cassandra.Mapping.MappingConfiguration();
        mappingConfig.Define(new global::Cassandra.Mapping.Map<RegistroCombate>()
            .TableName("telemetria_combate")    // Vinculación con la tabla física
            .KeyspaceName("holored")           // Definición del Keyspace para evitar colisiones
            .PartitionKey(u => u.SectorId)     // Clave de partición para distribución de carga
            .ClusteringKey(u => u.Fecha)       // Primera clave de agrupamiento (Clustering Key)
            .ClusteringKey(u => u.Timestamp)   // Segunda clave de agrupamiento para precisión temporal
            .Column(u => u.SectorId, cm => cm.WithName("sector_id"))
            .Column(u => u.Fecha, cm => cm.WithName("fecha"))
            .Column(u => u.Timestamp, cm => cm.WithName("timestamp"))
            .Column(u => u.NaveAtacante, cm => cm.WithName("nave_atacante"))
            .Column(u => u.NaveObjetivo, cm => cm.WithName("nave_objetivo"))
            .Column(u => u.DanoEscudos, cm => cm.WithName("danio_escudos")));

        _mapper = new global::Cassandra.Mapping.Mapper(session, mappingConfig);
    }

    /// <summary>
    /// Registra de forma asíncrona un impacto de combate.
    /// Implementa el requisito de absorción de miles de escrituras por segundo.
    /// </summary>
    /// <param name="impacto">Entidad con los datos técnicos del suceso.</param>
    /// <exception cref="Exception">Lanzada cuando no hay nodos disponibles en el clúster.</exception>
    public async Task RegistrarImpactoAsync(RegistroCombate impacto)
    {
        try
        {
            await _mapper.InsertAsync(impacto);
        }
        catch (NoHostAvailableException ex)
        {
            // Requisito 4: Control de excepciones específicas para evitar Service Unavailable
            throw new Exception("Interferencia detectada: El clúster de Cassandra no tiene nodos operativos.", ex);
        }
    }

    /// <summary>
    /// Recupera el historial de combate filtrado por sector y fecha.
    /// Optimizado mediante Partition Key para garantizar lecturas hiperveloces.
    /// </summary>
    /// <param name="sectorId">ID del sector (Partition Key).</param>
    /// <param name="fecha">Fecha del registro (Clustering Key).</param>
    /// <returns>Colección de registros que coinciden con los criterios de búsqueda.</returns>
    public async Task<IEnumerable<RegistroCombate>> ObtenerHistorialPorSectorAsync(string sectorId, DateTime fecha)
    {
        try
        {
            // Conversión al tipo nativo LocalDate de Cassandra para optimizar la consulta
            var cassandraFecha = new global::Cassandra.LocalDate(fecha.Year, fecha.Month, fecha.Day);

            // Consulta optimizada utilizando la clave primaria compuesta (sector_id + fecha)
            var query = global::Cassandra.Mapping.Cql.New(
                "SELECT * FROM holored.telemetria_combate WHERE sector_id = ? AND fecha = ?",
                sectorId,
                cassandraFecha);

            return await _mapper.FetchAsync<RegistroCombate>(query);
        }
        catch (QueryExecutionException ex)
        {
            // Manejo de errores durante la ejecución de consultas en el nodo Cassandra
            throw new Exception("Error crítico al procesar la consulta de telemetría táctica.", ex);
        }
    }
}