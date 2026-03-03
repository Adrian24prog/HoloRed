using HoloRed.Domain.Entities.Cassandra;
using HoloRed.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoloRed.Domain.Interfaces;

/// <summary>
/// Define el contrato de lógica de negocio para el procesamiento de inteligencia galáctica.
/// Orquesta la interacción entre los sistemas de telemetría de Cassandra y la red de espionaje de Neo4j.
/// </summary>
/// <remarks>
/// <author>Adrian Dondarza</author>
/// <date>03/03/2026</date>
/// </remarks>
public interface IInteligenciaService
{
    /// <summary>
    /// Procesa y persiste un evento de impacto en el motor de telemetría.
    /// Valida las reglas de negocio antes de la escritura masiva en el motor columnar.
    /// </summary>
    /// <param name="dto">Datos del impacto capturados por los sensores de combate.</param>
    /// <returns>Tarea asíncrona que representa la operación de registro.</returns>
    Task RegistrarEventoCombateAsync(ImpactoBatallaDto dto);

    /// <summary>
    /// Recupera el historial de eventos de combate transformado para su consumo externo.
    /// Realiza la conversión de entidades nativas de Cassandra a objetos de transferencia de datos (DTO).
    /// </summary>
    /// <param name="sectorId">Identificador del sector estelar (Partition Key).</param>
    /// <param name="fecha">Fecha de consulta para filtrar el historial (Clustering Key).</param>
    /// <returns>Colección de registros de combate optimizados para la API.</returns>
    Task<IEnumerable<RegistroCombateDto>> ObtenerHistorialAsync(string sectorId, DateTime fecha);

    /// <summary>
    /// Analiza los nodos de la red de espionaje para detectar traiciones.
    /// Ejecuta la lógica de búsqueda de múltiples saltos en el motor de grafos.
    /// </summary>
    /// <param name="origen">Facción de origen del presunto espía.</param>
    /// <param name="destino">Facción destino de la infiltración.</param>
    /// <returns>Lista de reportes con los nombres y ubicaciones de los infiltrados detectados.</returns>
    Task<IEnumerable<string>> AnalizarInfiltradosAsync(string origen, string destino);
}