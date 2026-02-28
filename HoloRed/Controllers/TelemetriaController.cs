using Microsoft.AspNetCore.Mvc;
using HoloRed.Dtos;
using HoloRed.Domain.Interfaces;
using HoloRed.Infrastructure.Cassandra;
using System;
using System.Threading.Tasks;

namespace HoloRed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelemetriaController : ControllerBase
    {
        private readonly IInteligenciaService _inteligenciaService;
        private readonly CassandraTelemetriaRepository _repository; // Mantenemos este solo para la consulta directa de historial

        /// <summary>
        /// Constructor que inyecta el servicio de inteligencia y el repositorio.
        /// </summary>
        /// <remarks>Autor: Adrian Dondarza</remarks>
        public TelemetriaController(IInteligenciaService inteligenciaService, CassandraTelemetriaRepository repository)
        {
            _inteligenciaService = inteligenciaService;
            _repository = repository;
        }

        /// <summary>
        /// POST /api/telemetria/impacto
        /// Registra un impacto de bláster masivo delegando la lógica al Service.
        /// </summary>
        [HttpPost("impacto")]
        public async Task<IActionResult> RegistrarImpacto([FromBody] ImpactoBatallaDto dto)
        {
            try
            {
                // Delegamos toda la lógica de conversión y guardado al servicio
                await _inteligenciaService.RegistrarEventoCombateAsync(dto);

                return Ok(new { mensaje = "Impacto registrado en los logs de la República a través del Service" });
            }
            catch (Exception ex)
            {
                // Cumplimos la rúbrica: Error 503 si el motor NoSQL no responde
                return StatusCode(503, "Sistemas de telemetría fuera de línea: " + ex.Message);
            }
        }

        /// <summary>
        /// GET /api/telemetria/historial/{sector}?fecha=YYYY-MM-DD
        /// Devuelve el registro de batalla de un sector en un día específico.
        /// </summary>
        [HttpGet("historial/{sector}")]
        public async Task<IActionResult> ObtenerHistorial(string sector, [FromQuery] DateTime fecha)
        {
            try
            {
                // Consulta directa optimizada por Partition Key
                var resultados = await _repository.ObtenerHistorialPorSectorAsync(sector, fecha.Date);
                return Ok(resultados);
            }
            catch (Exception)
            {
                return StatusCode(503, "Error al consultar la HoloRed");
            }
        }
    }
}