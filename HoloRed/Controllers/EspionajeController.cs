using Microsoft.AspNetCore.Mvc;
using HoloRed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoloRed.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EspionajeController : ControllerBase
{
    private readonly IInteligenciaService _inteligenciaService;

    /// <summary>
    /// Constructor que inyecta el servicio de inteligencia políglota.
    /// </summary>
    /// <remarks>Autor: Adrian Dondarza</remarks>
    public EspionajeController(IInteligenciaService inteligenciaService)
    {
        _inteligenciaService = inteligenciaService;
    }

    /// <summary>
    /// GET /api/espionaje/infiltrados?origen=Rebeldes&destino=Imperio
    /// Realiza una consulta de múltiples saltos en Neo4j para detectar espías.
    /// </summary>
    /// <param name="origen">Facción que envía al espía.</param>
    /// <param name="destino">Facción donde está infiltrado.</param>
    [HttpGet("infiltrados")]
    public async Task<IActionResult> GetInfiltrados([FromQuery] string origen, [FromQuery] string destino)
    {
        try
        {
            // Llamamos al servicio para ejecutar la consulta Cypher de grafos
            var reporte = await _inteligenciaService.AnalizarInfiltradosAsync(origen, destino);


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
            // Error 503 si el nodo de Neo4j no responde
            return StatusCode(503, "La red de espionaje está sufriendo interferencias: " + ex.Message);
        }
    }
}