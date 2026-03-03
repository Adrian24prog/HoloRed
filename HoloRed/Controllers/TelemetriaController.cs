using Microsoft.AspNetCore.Mvc;
using HoloRed.Dtos;
using HoloRed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoloRed.Controllers;

/// <summary>
/// Controlador de API para la gestión de telemetría de combate mediante el motor NoSQL Cassandra.
/// Orquesta el registro masivo de impactos y la consulta optimizada por sectores estelares.
/// </summary>
/// <remarks>
/// <author>Adrian Dondarza</author>
/// <date>02/03/2026</date>
/// </remarks>
[Route("api/[controller]")]
[ApiController]
public class TelemetriaController : ControllerBase
{
    private readonly IInteligenciaService _inteligenciaService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador inyectando el servicio de inteligencia.
    /// </summary>
    /// <param name="inteligenciaService">Interfaz del servicio de lógica de negocio políglota.</param>
    public TelemetriaController(IInteligenciaService inteligenciaService)
    {
        _inteligenciaService = inteligenciaService;
    }

    /// <summary>
    /// Registra un nuevo evento de combate de forma asíncrona en el motor de familias de columnas.
    /// Diseñado para absorber altas tasas de escritura (Append-only).
    /// </summary>
    /// <param name="dto">Objeto de transferencia de datos con los detalles del impacto.</param>
    /// <returns>Estado de la operación o error semántico 503 si el clúster no está disponible.</returns>
    [HttpPost("impacto")]
    public async Task<IActionResult> RegistrarImpacto([FromBody] ImpactoBatallaDto dto)
    {
        // Validación de esquema y tipos de datos de entrada
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                error = "Datos de telemetría corruptos",
                mensaje = "El formato del impacto no es válido. Verifique los tipos y atributos obligatorios.",
                detalles = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });
        }

        try
        {
            // Delegación de la persistencia a la capa de servicio
            await _inteligenciaService.RegistrarEventoCombateAsync(dto);

            return Ok(new
            {
                mensaje = $"¡Registro de impacto recibido en el sector {dto.SectorId}! " +
                          $"Los daños en el objetivo ({dto.NaveObjetivo}) han sido procesados correctamente."
            });
        }
        catch (Cassandra.NoHostAvailableException ex)
        {
            // Requisito 4: Captura de excepciones específicas del driver para evitar errores 500 genéricos
            return StatusCode(503, new
            {
                error = "Sistemas de telemetría fuera de línea",
                motivo = "El motor de datos masivos (Cassandra) no responde a las solicitudes de escritura.",
                detalles = ex.Message
            });
        }
        catch (Exception ex)
        {
            // Captura de errores de lógica de negocio o fallos inesperados
            return StatusCode(500, new
            {
                error = "Interferencia detectada en el puente de mando",
                mensaje = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtiene el historial de combate filtrado por sector y fecha.
    /// Utiliza el diseño de Primary Key de Cassandra para evitar escaneos completos (Full Scans).
    /// </summary>
    /// <param name="sector">Identificador único del sector estelar (Partition Key).</param>
    /// <param name="fecha">Fecha de la batalla en formato YYYY-MM-DD (Clustering Key).</param>
    /// <returns>Lista de registros de combate o 404 si no existen datos para ese filtro.</returns>
    [HttpGet("historial/{sector}")]
    public async Task<IActionResult> ObtenerHistorial(string sector, [FromQuery] DateTime fecha)
    {
        try
        {
            // Recuperación de datos optimizada por clave de partición
            var resultados = await _inteligenciaService.ObtenerHistorialAsync(sector, fecha);

            return Ok(resultados);
        }
        catch (KeyNotFoundException ex)
        {
            // Respuesta semántica: 404 para indicar que la consulta es válida pero no hay datos
            return NotFound(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            // Requisito 5: Error 503 ante la caída de nodos específicos de Cassandra durante la lectura
            return StatusCode(503, new
            {
                error = "Error al consultar la HoloRed táctica",
                detalle = "El nodo de datos Cassandra no está disponible para lectura.",
                excepcion = ex.Message
            });
        }
    }
}