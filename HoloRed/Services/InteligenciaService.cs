using HoloRed.Domain.Entities.Cassandra;
using HoloRed.Domain.Interfaces;
using HoloRed.Dtos;
using HoloRed.Infrastructure.Cassandra;
using Cassandra; // Necesario para la compatibilidad con LocalDate de Cassandra
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace HoloRed.Service;

/// <summary>
/// Capa de servicio encargada de la lógica de negocio central de la HoloRed.
/// Orquesta la interacción entre los motores NoSQL Cassandra (Telemetría masiva) 
/// y Neo4j (Análisis de grafos de espionaje).
/// </summary>
/// <remarks>
/// <author>Adrian Dondarza</author>
/// <date>03/03/2026</date>
/// </remarks>
public class InteligenciaService : IInteligenciaService
{
    private readonly CassandraTelemetriaRepository _cassandraRepo;
    private readonly IEspionajeRepository _neo4jRepo;

    /// <summary>
    /// Inicializa una nueva instancia del servicio inyectando los repositorios de persistencia.
    /// </summary>
    /// <param name="cassandraRepo">Repositorio para la gestión de logs de batalla.</param>
    /// <param name="neo4jRepo">Repositorio para el análisis de redes de infiltración.</param>
    public InteligenciaService(
        CassandraTelemetriaRepository cassandraRepo,
        IEspionajeRepository neo4jRepo)
    {
        _cassandraRepo = cassandraRepo;
        _neo4jRepo = neo4jRepo;
    }

    /// <summary>
    /// Procesa y persiste un evento de combate. Realiza la validación de integridad 
    /// y la conversión de tipos .NET a tipos nativos de Cassandra.
    /// </summary>
    /// <param name="dto">Objeto de transferencia con los datos del impacto bláster.</param>
    /// <exception cref="ArgumentException">Lanzada si los datos de telemetría son inconsistentes.</exception>
    public async Task RegistrarEventoCombateAsync(ImpactoBatallaDto dto)
    {
        // Validación de lógica de negocio: los daños en escudos deben ser positivos
        if (dto.DanoEscudos < 0)
        {
            throw new ArgumentException("Alerta de integridad: No se permiten registros con valores de daño negativos.");
        }

        // Conversión técnica de DateTime a LocalDate para optimizar el almacenamiento columnar
        var fechaCassandra = new LocalDate(dto.Fecha.Year, dto.Fecha.Month, dto.Fecha.Day);

        // Mapeo de DTO a Entidad de Dominio
        var entidad = new RegistroCombate
        {
            SectorId = dto.SectorId,
            Fecha = fechaCassandra,
            Timestamp = DateTimeOffset.Now, // Marca de tiempo precisa para el registro
            NaveAtacante = dto.NaveAtacante,
            NaveObjetivo = dto.NaveObjetivo,
            DanoEscudos = dto.DanoEscudos
        };

        await _cassandraRepo.RegistrarImpactoAsync(entidad);
    }

    /// <summary>
    /// Obtiene el historial de combate optimizado. Valida la existencia de datos 
    /// y transforma las entidades en DTOs para su exposición en la API.
    /// </summary>
    /// <param name="sectorId">Identificador del sector estelar (Partition Key).</param>
    /// <param name="fecha">Fecha de consulta (Clustering Key).</param>
    /// <returns>Colección de DTOs con la información de los impactos registrados.</returns>
    /// <exception cref="KeyNotFoundException">Lanzada si el sector no presenta registros el día solicitado.</exception>
    public async Task<IEnumerable<RegistroCombateDto>> ObtenerHistorialAsync(string sectorId, DateTime fecha)
    {
        // Consulta al repositorio utilizando la fecha sin componentes de hora
        var resultados = await _cassandraRepo.ObtenerHistorialPorSectorAsync(sectorId, fecha.Date);

        // Verificación de existencia de registros para evitar respuestas vacías ambiguas
        if (resultados == null || !resultados.Any())
        {
            throw new KeyNotFoundException($"Informe táctico: El sector {sectorId} no presenta anomalías el día {fecha:yyyy-MM-dd}. (No existe)");
        }

        // Proyección de Entidades nativas a DTOs de salida (Desacoplamiento de infraestructura)
        return resultados.Select(r => new RegistroCombateDto
        {
            SectorId = r.SectorId,
            // Formateo de fecha para consumo estándar JSON
            Fecha = $"{r.Fecha.Year}-{r.Fecha.Month:D2}-{r.Fecha.Day:D2}",
            Timestamp = r.Timestamp,
            NaveAtacante = r.NaveAtacante,
            NaveObjetivo = r.NaveObjetivo,
            DanioEscudos = r.DanoEscudos
        });
    }

    /// <summary>
    /// Orquesta el análisis de infiltración delegando la consulta de grafos al motor Neo4j.
    /// </summary>
    /// <param name="origen">Facción de origen del reporte.</param>
    /// <param name="destino">Facción objetivo del análisis.</param>
    /// <returns>Lista de espías detectados en la red de inteligencia.</returns>
    public async Task<IEnumerable<string>> AnalizarInfiltradosAsync(string origen, string destino)
    {
        return await _neo4jRepo.ObtenerInfiltradosAsync(origen, destino);
    }
}