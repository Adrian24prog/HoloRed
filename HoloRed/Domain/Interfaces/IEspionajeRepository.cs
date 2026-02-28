using HoloRed.Domain.Entities;

namespace HoloRed.Domain.Interfaces;

/// <summary>
/// Interfaz para consultas de grafos en Neo4j.
/// </summary>
/// <remarks>Autor: Adrian Dondarza</remarks>
public interface IEspionajeRepository
{
    /// <summary>
    /// Realiza una consulta de "múltiples saltos" para encontrar espías infiltrados.
    /// </summary>
    Task<IEnumerable<string>> ObtenerInfiltradosAsync(string faccionOrigen, string faccionDestino);
}