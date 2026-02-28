using Cassandra.Mapping;
using HoloRed.Domain.Entities.Cassandra;
using HoloRed.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoloRed.Infrastructure.Cassandra;

/// <summary>
/// Repositorio encargado de la persistencia en el motor columnar Cassandra.
/// Implementa escrituras masivas y consultas por sector/fecha.
/// </summary>
/// <remarks>
/// Autor: Adrian Dondarza
/// </remarks>
public class CassandraTelemetriaRepository
{
// Usamos global:: para forzar al compilador a usar la librería externa y no las carpetas locales
private readonly global::Cassandra.ISession _session;
private readonly global::Cassandra.Mapping.IMapper _mapper;

/// <summary>
/// Constructor que recibe la sesión de Cassandra inyectada.
/// </summary>
/// <param name="session">Sesión activa del clúster de Cassandra.</param>
public CassandraTelemetriaRepository(global::Cassandra.ISession session)
{
    _session = session;
    // El Mapper es el que nos permite trabajar con objetos en lugar de consultas SQL puras
    _mapper = new global::Cassandra.Mapping.Mapper(session);
}

/// <summary>
/// Inserta un impacto de bláster de forma asíncrona en la tabla columnar.
/// </summary>
/// <param name="impacto">Entidad con los datos de telemetría a persistir.</param>
public async Task RegistrarImpactoAsync(RegistroCombate impacto)
{
    await _mapper.InsertAsync(impacto);
}

/// <summary>
/// Obtiene el historial de combate de un sector sin realizar un Full Scan.
/// La consulta es hiperveloz gracias al uso de Partition Key (sector_id) y Clustering Key (fecha).
/// </summary>
/// <param name="sectorId">ID del sector a consultar.</param>
/// <param name="fecha">Fecha específica de la batalla.</param>
/// <returns>Lista de registros de combate encontrados.</returns>
public async Task<IEnumerable<RegistroCombate>> ObtenerHistorialPorSectorAsync(string sectorId, DateTime fecha)
{
    // Usamos FetchAsync para obtener la colección de datos que coincidan con la clave primaria
    return await _mapper.FetchAsync<RegistroCombate>(
        "WHERE sector_id = ? AND fecha = ?", sectorId, fecha);
}
}