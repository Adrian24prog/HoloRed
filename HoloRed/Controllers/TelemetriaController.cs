using Microsoft.AspNetCore.Mvc;
using HoloRed.Dtos;
using HoloRed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoloRed.Controllers
{
    /// <summary>
    /// Controlador de API para la gestión de telemetría de combate en Cassandra.
    /// Centraliza las operaciones de registro y consulta de impactos estelares.
    /// </summary>
    /// <remarks>
    /// Autor: Adrian Dondarza
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class TelemetriaController : ControllerBase
    {
        private readonly IInteligenciaService _inteligenciaService;

        /// <summary>
        /// Constructor con inyección del servicio de inteligencia.
        /// </summary>
        public TelemetriaController(IInteligenciaService inteligenciaService)
        {
            _inteligenciaService = inteligenciaService;
        }

        /// <summary>
        /// POST /api/telemetria/impacto
        /// Registra un nuevo evento de combate en el motor NoSQL Cassandra.
        /// </summary>
        /// <param name="dto">Datos del impacto (sector, naves, daño, fecha).</param>
        /// <returns>Mensaje de confirmación o error 503 si el servicio falla.</returns>
        [HttpPost("impacto")]
        public async Task<IActionResult> RegistrarImpacto([FromBody] ImpactoBatallaDto dto)
        {
            try
            {
                // Delegamos la lógica de negocio y conversión al servicio
                await _inteligenciaService.RegistrarEventoCombateAsync(dto);

                return Ok(new { mensaje = "Impacto registrado en los logs de la República a través del Service" });
            }
            catch (Exception ex)
            {
                // Error 503: Servicio no disponible (Rúbrica: Fallo en motor NoSQL)
                return StatusCode(503, "Sistemas de telemetría fuera de línea: " + ex.Message);
            }
        }

        /// <summary>
        /// GET /api/telemetria/historial/{sector}?fecha=YYYY-MM-DD
        /// Consulta el historial de impactos filtrado por sector y fecha exacta.
        /// </summary>
        /// <param name="sector">ID del sector estelar.</param>
        /// <param name="fecha">Fecha de la batalla (formato YYYY-MM-DD).</param>
        /// <returns>Lista de impactos o 404 con mensaje personalizado si no hay datos.</returns>
        [HttpGet("historial/{sector}")]
        public async Task<IActionResult> ObtenerHistorial(string sector, [FromQuery] DateTime fecha)
        {
            try
            {
                // Solicitamos los datos al servicio. 
                // Si no hay resultados, el servicio lanzará una KeyNotFoundException.
                var resultados = await _inteligenciaService.ObtenerHistorialAsync(sector, fecha);

                return Ok(resultados);
            }
            catch (KeyNotFoundException ex)
            {
                // RESPUESTA SEMÁNTICA: Devolvemos 404 con el mensaje de "No encontrado"
                // Esto ocurre cuando la búsqueda es válida pero la lista está vacía.
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                // Error 503: Indica problemas técnicos con el nodo de Cassandra.
                return StatusCode(503, "Error al consultar la HoloRed (Posible caída del nodo Cassandra): " + ex.Message);
            }
        }
    }
}