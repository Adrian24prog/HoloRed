using HoloRed.Domain.Entities.Cassandra;
using HoloRed.Dtos;

namespace HoloRed.Domain.Interfaces;

/// <summary>
/// Define la lógica de negocio para el análisis de inteligencia.
/// </summary>
/// <remarks>Autor: Adrian Dondarza</remarks>
public interface IInteligenciaService
{
    Task RegistrarEventoCombateAsync(ImpactoBatallaDto dto);

    // CAMBIA ESTO: De RegistroCombate a RegistroCombateDto
    Task<IEnumerable<RegistroCombateDto>> ObtenerHistorialAsync(string sectorId, DateTime fecha);

    Task<IEnumerable<string>> AnalizarInfiltradosAsync(string origen, string destino);
}