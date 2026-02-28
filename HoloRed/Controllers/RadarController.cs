using HoloRed.Domain.Interfaces;
using HoloRed.Dtos;
using HoloRed.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace HoloRed.Controllers;

/// <summary>
/// Controlador principal del sistema de Radar y Control Espacial.
/// Gestiona la recepción de señales de balizas y coordina los procesos
/// de atraque asegurando que solo naves detectadas puedan ocupar bahías.
/// </summary>
/// <author>Álvaro Naranjo</author>
/// <date>28/02/2026</date>
[ApiController]
[Route("api/[controller]")]
public class RadarController : ControllerBase
{
    private readonly IRadarRepository _radarRepo;
    private readonly AtraqueService _atraqueService;

    /// <summary>
    /// Constructor con inyección de dependencias para el repositorio de radar y servicio de atraque.
    /// </summary>
    /// <param name="radarRepo">Interfaz de acceso a datos en Redis.</param>
    /// <param name="atraqueService">Servicio lógico de gestión de bahías.</param>
    public RadarController(IRadarRepository radarRepo, AtraqueService atraqueService)
    {
        _radarRepo = radarRepo;
        _atraqueService = atraqueService;
    }

    /// <summary>
    /// Registra o actualiza la señal de una nave en el radar.
    /// Si la nave ya existe, renueva su tiempo de vida (TTL) en el sistema.
    /// </summary>
    /// <param name="codigoNave">Identificador único de la nave (ej. Falcon-1).</param>
    /// <param name="dto">Objeto que contiene el estado actual de la nave.</param>
    /// <returns>200 OK si la señal se procesó correctamente, 503 si Redis no responde.</returns>
    [HttpPost("baliza/{codigoNave}")]
    public async Task<IActionResult> ActualizarBaliza(string codigoNave, [FromBody] BalizaUpdateDto dto)
    {
        try
        {
            // Registramos la actividad en Redis con expiración automática
            await _radarRepo.ActualizarBalizaAsync(codigoNave, dto.Estado);
            return Ok(new { message = $"Señal de {codigoNave} recibida. TTL renovado." });
        }
        catch (RedisConnectionException)
        {
            // Error controlado si el servidor de persistencia no está disponible
            return StatusCode(503, new { error = "Servicio de Radar (Redis) no disponible temporalmente." });
        }
    }

    /// <summary>
    /// Solicita el atraque de una nave en una bahía específica.
    /// Valida primero que la nave esté activa en el radar antes de proceder.
    /// </summary>
    /// <param name="numeroBahia">Número de la bahía deseada.</param>
    /// <param name="codigoNave">Nombre de la nave que solicita el espacio.</param>
    /// <returns>
    /// 200 OK: Atraque exitoso.
    /// 404 Not Found: La nave no ha enviado baliza previa.
    /// 409 Conflict: La bahía ya está ocupada por otra nave.
    /// </returns>
    [HttpPost("atraque/{numeroBahia}")]
    public async Task<IActionResult> SolicitarAtraque(int numeroBahia, [FromQuery] string codigoNave)
    {
        //  VALIDACIÓN DE SEGURIDAD: Comprobar si la nave existe en Redis
        bool existeEnRadar = await _radarRepo.ExisteNaveAsync(codigoNave);

        if (!existeEnRadar)
        {
            return NotFound(new
            {
                message = $"Error: La nave '{codigoNave}' no figura en el radar activo. Debe enviar una baliza antes de atracar."
            });
        }

        // CONTROL DE CONCURRENCIA: El servicio usa SemaphoreSlim para evitar colisiones
        bool exitoAtraque = await _atraqueService.IntentarAtraqueAsync(numeroBahia, codigoNave);

        if (!exitoAtraque)
        {
            return Conflict(new { message = $"Alerta de Colisión: La bahía {numeroBahia} ya se encuentra ocupada." });
        }

        return Ok(new { message = $"Maniobra completada: La nave {codigoNave} ha atracado en la bahía {numeroBahia}." });
    }
}