using System;
using Cassandra; // Necesario para el mapeo de tipos específicos de Cassandra

namespace HoloRed.Dtos;

/// <summary>
/// Objeto de Transferencia de Datos (DTO) para el registro de impactos de combate.
/// Se utiliza para transportar la información desde el controlador hasta el motor de Cassandra.
/// </summary>
/// <remarks>
/// Autor: Adrian Dondarza
/// </remarks>
public class ImpactoBatallaDto
{
    /// <summary>
    /// Identificador único del sector estelar donde ocurrió el evento.
    /// Se utiliza como Partition Key en la base de datos para distribuir los datos.
    /// </summary>
    public string SectorId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador o nombre de la nave que realiza el disparo (ej: X-Wing, TIE Fighter).
    /// </summary>
    public string NaveAtacante { get; set; } = string.Empty;

    /// <summary>
    /// Identificador o nombre de la nave que recibe el impacto.
    /// </summary>
    public string NaveObjetivo { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de daño reducido de los escudos deflectores tras el impacto.
    /// </summary>
    public int DanoEscudos { get; set; }

    /// <summary>
    /// Fecha y hora exacta del suceso. 
    /// NOTA: En el repositorio se debe mapear a Cassandra.LocalDate para evitar el error de 4/8 bytes.
    /// </summary>
    public DateTime Fecha { get; set; } = DateTime.Now;
}