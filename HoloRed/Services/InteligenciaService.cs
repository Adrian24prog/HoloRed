using HoloRed.Domain.Entities.Cassandra;
using HoloRed.Domain.Interfaces;
using HoloRed.Dtos;
using HoloRed.Infrastructure.Cassandra;
using Cassandra; // Necesario para el tipo LocalDate
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace HoloRed.Service
{
    /// <summary>
    /// Capa de servicio encargada de la lógica de negocio de la HoloRed.
    /// Orquesta las operaciones entre los motores Cassandra (Telemetría) y Neo4j (Espionaje).
    /// </summary>
    /// <remarks>
    /// Autor: Adrian Dondarza
    /// </remarks>
    public class InteligenciaService : IInteligenciaService
    {
        private readonly CassandraTelemetriaRepository _cassandraRepo;
        private readonly IEspionajeRepository _neo4jRepo;

        public InteligenciaService(
            CassandraTelemetriaRepository cassandraRepo,
            IEspionajeRepository neo4jRepo)
        {
            _cassandraRepo = cassandraRepo;
            _neo4jRepo = neo4jRepo;
        }

        /// <summary>
        /// Procesa y persiste un evento de combate convirtiendo los tipos de C# a Cassandra.
        /// </summary>
        public async Task RegistrarEventoCombateAsync(ImpactoBatallaDto dto)
        {
            var fechaCassandra = new LocalDate(dto.Fecha.Year, dto.Fecha.Month, dto.Fecha.Day);

            var entidad = new RegistroCombate
            {
                SectorId = dto.SectorId,
                Fecha = fechaCassandra,
                Timestamp = DateTimeOffset.Now,
                NaveAtacante = dto.NaveAtacante,
                NaveObjetivo = dto.NaveObjetivo,
                DanioEscudos = dto.DañoEscudos
            };

            await _cassandraRepo.RegistrarImpactoAsync(entidad);
        }

        /// <summary>
        /// Obtiene el historial y valida si existen resultados.
        /// Si la lista está vacía, lanza una excepción capturable por el controlador.
        /// </summary>
        public async Task<IEnumerable<RegistroCombateDto>> ObtenerHistorialAsync(string sectorId, DateTime fecha)
        {
            var resultados = await _cassandraRepo.ObtenerHistorialPorSectorAsync(sectorId, fecha.Date);

            if (resultados == null || !resultados.Any())
            {
                throw new KeyNotFoundException($"No hay registros para el sector {sectorId} el día {fecha:yyyy-MM-dd}.");
            }

            // Convertimos la entidad de Cassandra a DTO compatible con JSON/Swagger
            return resultados.Select(r => new RegistroCombateDto
            {
                SectorId = r.SectorId,
                // Convertimos LocalDate a string legible
                Fecha = $"{r.Fecha.Year}-{r.Fecha.Month:D2}-{r.Fecha.Day:D2}",
                Timestamp = r.Timestamp,
                NaveAtacante = r.NaveAtacante,
                NaveObjetivo = r.NaveObjetivo,
                DanioEscudos = r.DanioEscudos
            });
        }

        public async Task<IEnumerable<string>> AnalizarInfiltradosAsync(string origen, string destino)
        {
            return await _neo4jRepo.ObtenerInfiltradosAsync(origen, destino);
        }
    }
}