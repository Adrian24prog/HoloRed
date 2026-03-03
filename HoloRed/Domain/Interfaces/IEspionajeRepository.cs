using HoloRed.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoloRed.Domain.Interfaces;

/// <summary>
/// Interfaz para la gestión de la red de inteligencia galáctica mediante Neo4j.
/// Define las operaciones necesarias para desentrañar alianzas y traiciones 
/// donde la relación entre entidades es crítica.
/// </summary>
/// <remarks>
/// <author>Adrian Dondarza</author>
/// <date>03/03/2026</date>
/// </remarks>
public interface IEspionajeRepository
{
    /// <summary>
    /// Ejecuta una consulta Cypher de "múltiples saltos" en el motor de grafos.
    /// Identifica agentes que poseen una relación de origen y operan en un destino rival,
    /// cumpliendo con el requisito de profundidad en el análisis de inteligencia.
    /// </summary>
    /// <param name="faccionOrigen">Facción que emite o controla al agente.</param>
    /// <param name="faccionDestino">Facción objetivo donde se sospecha la infiltración.</param>
    /// <returns>Colección de identificadores o reportes de agentes detectados.</returns>
    Task<IEnumerable<string>> ObtenerInfiltradosAsync(string faccionOrigen, string faccionDestino);
}