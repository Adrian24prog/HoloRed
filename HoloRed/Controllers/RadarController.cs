using HoloRed.Domain.Interfaces;
using HoloRed.Dtos;
using HoloRed.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace HoloRed.Controllers;

/// <summary>
/// Controlador principal del sistema de Radar y Control Espacial.
/// Gestiona la recepción de señales de balizas y coordina los procesos
/// de atraque asegurando que solo naves detectadas puedan ocupar bahías.
/// </summary>
/// <author>Álvaro Naranjo</author>
/// <date>02/03/2026</date>
[ApiController]
[Route("api/[controller]")]
public class RadarController : ControllerBase
{
    private readonly IRadarRepository _radarRepo;
    private readonly AtraqueService _atraqueService;

    public RadarController(IRadarRepository radarRepo, AtraqueService atraqueService)
    {
        _radarRepo = radarRepo;
        _atraqueService = atraqueService;
    }

    /// <summary>
    /// Registra o actualiza la señal de una nave en el radar.
    /// </summary>
    /// <returns>200 OK, o 503 si el motor de memoria falla.</returns>
    [HttpPost("baliza/{codigoNave}")]
    public async Task<IActionResult> ActualizarBaliza(string codigoNave, [FromBody] BalizaUpdateDto dto)
    {
        try
        {
            await _radarRepo.ActualizarBalizaAsync(codigoNave, dto.Estado);
            return Ok(new { message = $"Señal de {codigoNave} recibida. TTL renovado satisfactoriamente." });
        }
        catch (Exception ex) when (ex is RedisConnectionException || ex is RedisTimeoutException || ex is RedisServerException)
        {
            // Captura de múltiples excepciones específicas de Redis para devolver un 503 semántico
            return StatusCode(503, new
            {
                error = "Servicio de Radar (Redis) no disponible o saturado.",
                detalle = "Interferencias en la HoloRed detectadas."
            });
        }
    }

    /// <summary>
    /// Proceso crítico de atraque con validación previa en Radar.
    /// </summary>
    [HttpPost("atraque/{numeroBahia}")]
    public async Task<IActionResult> SolicitarAtraque(int numeroBahia, [FromQuery] string codigoNave)
    {
        try
        {
            // 1. Validación de existencia en motor Clave-Valor
            bool existeEnRadar = await _radarRepo.ExisteNaveAsync(codigoNave);

            if (!existeEnRadar)
            {
                return NotFound(new
                {
                    message = $"Error: La nave '{codigoNave}' no figura en el radar activo. Debe emitir señal de baliza antes del atraque."
                });
            }

            // 2. Ejecución de maniobra con control de concurrencia (SemaphoreSlim)
            bool exitoAtraque = await _radarRepo.AsignarAtraqueAsync(numeroBahia, codigoNave);

            if (!exitoAtraque)
            {
                // Devolvemos 409 Conflict si la bahía ya está ocupada por otro hilo/nave
                return Conflict(new { message = $"Alerta de Colisión: La bahía {numeroBahia} ya está asignada a otra nave." });
            }

            return Ok(new { message = $"Maniobra completada: La nave {codigoNave} está segura en la bahía {numeroBahia}." });
        }
        catch (Exception ex) when (ex is RedisConnectionException || ex is RedisTimeoutException)
        {
            // Respuesta adecuada ante caída del motor NoSQL
            return StatusCode(503, new { message = "El sistema de radar no responde. Maniobra de atraque abortada por seguridad." });
        }
        catch (Exception ex)
        {
            // Captura genérica para evitar el cierre de la API
            return StatusCode(500, new { message = "Error interno en los sistemas de atraque.", info = ex.Message });
        }
    }
}