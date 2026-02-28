using Microsoft.AspNetCore.Mvc;
using HoloRed.Infrastructure.Repositories;
using HoloRed.Services;
using HoloRed.Dtos;
using StackExchange.Redis;

namespace HoloRed.Controllers;

/// <summary>
/// Controlador API para la gestión del Radar espacial.
/// Permite actualizar balizas en tiempo real (Redis) y gestionar
/// solicitudes de atraque en bahías espaciales.
/// </summary>
/// <author>Álvaro Naranjo</author>
/// <date>28/02/2026</date>
[ApiController]
[Route("api/[controller]")]
public class RadarController : ControllerBase
{
    private readonly RedisRadarRepository _radarRepo;
    private readonly AtraqueService _atraqueService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de Radar
    /// con sus dependencias inyectadas.
    /// </summary>
    /// <param name="radarRepo">Repositorio Redis para la gestión de balizas.</param>
    /// <param name="atraqueService">Servicio encargado de la lógica de atraque.</param>
    public RadarController(RedisRadarRepository radarRepo, AtraqueService atraqueService)
    {
        _radarRepo = radarRepo;
        _atraqueService = atraqueService;
    }

    /// <summary>
    /// Recibe y actualiza la baliza de una nave espacial,
    /// renovando su TTL de 10 minutos en Redis.
    /// </summary>
    /// <param name="codigoNave">Identificador único de la nave.</param>
    /// <param name="dto">Objeto DTO que contiene el nuevo estado de la nave.</param>
    /// <returns>
    /// 200 OK si la señal fue registrada correctamente.  
    /// 503 ServiceUnavailable si Redis no está disponible.  
    /// 500 InternalServerError ante error inesperado.
    /// </returns>
    [HttpPost("baliza/{codigoNave}")]
    public async Task<IActionResult> ActualizarBaliza(string codigoNave, [FromBody] BalizaUpdateDto dto)
    {
        try
        {
            await _radarRepo.ActualizarBalizaAsync(codigoNave, dto.Estado);

            return Ok(new
            {
                message = $"Señal de {codigoNave} recibida. TTL renovado 10 min."
            });
        }
        catch (RedisConnectionException)
        {
            return StatusCode(503, new
            {
                error = "Servicio de Radar (Redis) temporalmente no disponible."
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                error = "Error interno inesperado en el sistema de Radar."
            });
        }
    }

    /// <summary>
    /// Solicita el atraque de una nave en una bahía específica.
    /// Implementa control de concurrencia para evitar colisiones.
    /// </summary>
    /// <param name="numeroBahia">Número identificador de la bahía espacial.</param>
    /// <param name="codigoNave">Identificador único de la nave solicitante.</param>
    /// <returns>
    /// 200 OK si el atraque fue exitoso.  
    /// 409 Conflict si la bahía ya está reservada.  
    /// 500 InternalServerError ante error inesperado.
    /// </returns>
    [HttpPost("atraque/{numeroBahia}")]
    public async Task<IActionResult> SolicitarAtraque(int numeroBahia, [FromQuery] string codigoNave)
    {
        try
        {
            bool exito = await _atraqueService.IntentarAtraqueAsync(numeroBahia, codigoNave);

            if (!exito)
            {
                return Conflict(new
                {
                    message = $"¡ALERTA! La bahía {numeroBahia} ya está reservada. Maniobra abortada."
                });
            }

            return Ok(new
            {
                message = $"Atraque de {codigoNave} en bahía {numeroBahia} completado con éxito."
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                error = "Error interno durante el proceso de atraque."
            });
        }
    }
}