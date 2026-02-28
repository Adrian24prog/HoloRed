using Microsoft.AspNetCore.Mvc;
using HoloRed.Dtos;
using HoloRed.Infrastructure.Cassandra;
using HoloRed.Domain.Entities;

namespace HoloRed.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TelemetriaController : ControllerBase
{
    private readonly CassandraTelemetriaRepository _repository;

    /// <summary>
    /// Constructor que inyecta el repositorio de Cassandra.
    /// </summary>
    /// <remarks>Autor: Adrian Dondarza</remarks>
    public TelemetriaController(CassandraTelemetriaRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// POST /api/telemetria/impacto
    /// Registra un impacto de bláster masivo en tiempo real.
    /// </summary>
    [HttpPost("impacto")]
    public async Task<IActionResult> RegistrarImpacto([FromBody] ImpactoBatallaDto dto)
    {
        try
        {
            // Convertimos el DTO a la Entidad de Cassandra
            var entidad = new RegistroCombate
            {
                SectorId = dto.SectorId,
                Fecha = dto.Fecha.Date, // Solo la parte de la fecha
                Timestamp = DateTimeOffset.Now,
                NaveAtacante = dto.NaveAtacante,
                NaveObjetivo = dto.NaveObjetivo,
                DañoEscudos = dto.DañoEscudos
            };

            await _repository.RegistrarImpactoAsync(entidad);
            return Ok(new { mensaje = "Impacto registrado en los logs de la República" });
        }
        catch (Exception ex)
        {
            // Si Cassandra cae, devolvemos 503 como pide la rúbrica
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
            var resultados = await _repository.ObtenerHistorialPorSectorAsync(sector, fecha.Date);
            return Ok(resultados);
        }
        catch (Exception)
        {
            return StatusCode(503, "Error al consultar la HoloRed");
        }
    }
}