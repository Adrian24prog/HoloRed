using HoloRed.Domain.Entities.Cassandra;
using HoloRed.Domain.Interfaces;
using HoloRed.Dtos;
using HoloRed.Infrastructure.Cassandra;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        /// <summary>
        /// Constructor con inyección de dependencias para los repositorios políglotas.
        /// </summary>
        public InteligenciaService(
            CassandraTelemetriaRepository cassandraRepo,
            IEspionajeRepository neo4jRepo)
        {
            _cassandraRepo = cassandraRepo;
            _neo4jRepo = neo4jRepo;
        }

        /// <summary>
        /// Procesa y persiste un evento de combate. 
        /// Aquí se aplica la lógica de transformación del DTO a la Entidad de Cassandra.
        /// </summary>
        /// <param name="dto">Datos de entrada desde la API.</param>
        public async Task RegistrarEventoCombateAsync(ImpactoBatallaDto dto)
        {
            // Lógica de negocio: Validamos o transformamos datos antes de guardar
            // Por ejemplo: Forzamos que la fecha sea solo el componente 'Date' para la Partition Key
            var entidad = new RegistroCombate
            {
                SectorId = dto.SectorId,
                Fecha = dto.Fecha.Date,
                Timestamp = DateTimeOffset.Now, // Generamos el timestamp exacto del registro
                NaveAtacante = dto.NaveAtacante,
                NaveObjetivo = dto.NaveObjetivo,
                DañoEscudos = dto.DañoEscudos
            };

            // Delegamos la persistencia al repositorio de Cassandra
            await _cassandraRepo.RegistrarImpactoAsync(entidad);
        }

        /// <summary>
        /// Consulta la red de grafos para identificar agentes infiltrados entre facciones.
        /// </summary>
        /// <param name="origen">Facción de los espías (ej. Rebeldes).</param>
        /// <param name="destino">Facción objetivo (ej. Imperio).</param>
        /// <returns>Lista de strings con información de infiltración.</returns>
        public async Task<IEnumerable<string>> AnalizarInfiltradosAsync(string origen, string destino)
        {
            // Llamada directa al motor de grafos Neo4j
            return await _neo4jRepo.ObtenerInfiltradosAsync(origen, destino);
        }
    }
}