using Cassandra;
using Cassandra.Mapping;
using HoloRed.Domain.Entities;
using HoloRed.Dtos;

namespace HoloRed.Infrastructure.Cassandra;

/// <summary>
/// Repositorio encargado de la persistencia en el motor columnar Cassandra.
/// Implementa escrituras masivas y consultas por sector/fecha.
/// </summary>
/// <remarks>Autor: Adrian Dondarza</remarks>
public class CassandraTelemetriaRepository
{
    private readonly ISession _session;
    private readonly IMapper _mapper;

    public CassandraTelemetriaRepository(ISession session)
    {
        _session = session;
        _mapper = new Mapper(session);
    }

    /// <summary>
    /// Inserta un impacto de bláster de forma asíncrona.
    /// </summary>
    public async Task RegistrarImpactoAsync(RegistroCombate impacto)
    {
        await _mapper.InsertAsync(impacto);
    }

    /// <summary>
    /// Obtiene el historial sin realizar un Full Scan gracias al diseño de la Partition Key.
    /// </summary>
    public async Task<IEnumerable<RegistroCombate>> ObtenerHistorialPorSectorAsync(string sectorId, DateTime fecha)
    {
        // Consulta nativa optimizada por el diseño de tabla
        return await _mapper.FetchAsync<RegistroCombate>(
            "WHERE sector_id = ? AND fecha = ?", sectorId, fecha);
    }
}
