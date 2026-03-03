using System;

namespace HoloRed.Dtos;

/// <summary>
/// Objeto de Transferencia de Datos (DTO) para la captura de telemetría de combate.
/// Facilita el transporte de métricas de daño desde los sensores de la flota 
/// hacia el motor de persistencia masiva Cassandra.
/// </summary>
/// <remarks>
/// <author>Adrian Dondarza</author>
/// <date>03/03/2026</date>
/// </remarks>
public class ImpactoBatallaDto
{
    /// <summary>
    /// Identificador del sector estelar donde se ha detectado la anomalía de combate.
    /// Este campo actúa como la Clave de Partición (Partition Key) para optimizar 
    /// la distribución de carga en el clúster P2P.
    /// </summary>
    public string SectorId { get; set; } = string.Empty;

    /// <summary>
    /// Firma técnica de la unidad agresora identificada por los sistemas de inteligencia.
    /// </summary>
    public string NaveAtacante { get; set; } = string.Empty;

    /// <summary>
    /// Identificador de la unidad aliada o civil que ha recibido el impacto bláster.
    /// </summary>
    public string NaveObjetivo { get; set; } = string.Empty;

    /// <summary>
    /// Valor entero que cuantifica la reducción de potencia en los escudos deflectores.
    /// Requisito de telemetría: Absorción de miles de escrituras por segundo.
    /// </summary>
    public int DanoEscudos { get; set; }

    /// <summary>
    /// Marca temporal del suceso. 
    /// En la capa de servicio se realiza la conversión a LocalDate para garantizar 
    /// la compatibilidad con el almacenamiento columnar de Cassandra.
    /// </summary>
    public DateTime Fecha { get; set; } = DateTime.Now;
}