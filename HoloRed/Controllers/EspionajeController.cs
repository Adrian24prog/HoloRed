using Microsoft.AspNetCore.Mvc;
using HoloRed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoloRed.Controllers;

/// <summary>
/// Controlador encargado de gestionar la red de inteligencia galáctica.
/// Utiliza consultas de grafos en Neo4j para identificar agentes infiltrados 
/// y traiciones entre distintas facciones de la galaxia.
/// </summary>
/// <remarks>
/// <author>Adrian Dondarza</author>
/// <date>02/03/2026</date>
/// </remarks>
[Route("api/[controller]")]
[ApiController]
public class EspionajeController : ControllerBase
{
    private readonly IInteligenciaService _inteligenciaService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador inyectando el servicio de inteligencia.
    /// </summary>
    /// <param name="inteligenciaService">Servicio que orquestas las consultas políglotas.</param>
    public EspionajeController(IInteligenciaService inteligenciaService)
    {
        _inteligenciaService = inteligenciaService;
    }

    /// <summary>
    /// Consulta agentes infiltrados que operan entre dos facciones específicas.
    /// Realiza un análisis de múltiples saltos en la base de datos de grafos (Neo4j).
    /// </summary>
    /// <param name="origen">Nombre de la facción que presuntamente envía al espía.</param>
    /// <param name="destino">Nombre de la facción donde se sospecha que está la infiltración.</param>
    /// <returns>
    /// 200 OK con la lista de agentes encontrados.
    /// 404 Not Found si no se detectan traidores en la red.
    /// 503 Service Unavailable si hay un fallo en el motor de grafos.
    /// </returns>
    [HttpGet("infiltrados")]
    public async Task<IActionResult> GetInfiltrados([FromQuery] string origen, [FromQuery] string destino)
    {
        try
        {
            // Ejecutamos el análisis de profundidad en el motor de grafos
            var reporte = await _inteligenciaService.AnalizarInfiltradosAsync(origen, destino);

            // Validamos si la red ha devuelto algún agente comprometido
            if (reporte == null || !reporte.Any())
            {
                return NotFound(new
                {
                    FechaConsulta = DateTime.Now,
                    mensaje = $"Informe de Inteligencia: No se han detectado agentes de {origen} colaborando con {destino}. El sector parece estar limpio.",
                    codigo = "CLEAR_SECTOR"
                });
            }

            return Ok(new
            {
                FechaConsulta = DateTime.Now,
                Resultado = reporte
            });
        }
        catch (Exception ex)
        {
            // Implementación del requisito de robustez: captura de fallos en el driver de Neo4j.
            // Se devuelve 503 para indicar que el servicio de persistencia no está disponible temporalmente.
            return StatusCode(503, new
            {
                error = "La red de espionaje está sufriendo interferencias críticas en el motor de grafos.",
                detalle = ex.Message
            });
        }
    }
}