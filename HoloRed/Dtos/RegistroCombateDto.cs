using System;

namespace HoloRed.Dtos;

/// <summary>
/// Objeto de Transferencia de Datos (DTO) para la salida de registros de telemetría.
/// Diseñado para transformar los tipos nativos de Cassandra en formatos estándar 
/// compatibles con la interfaz de usuario y sistemas de visualización.
/// </summary>
/// <remarks>
/// <author>Adrian Dondarza</author>
/// <date>03/03/2026</date>
/// </remarks>
public class RegistroCombateDto
{
    /// <summary>
    /// Identificador del sector estelar consultado. 
    /// Corresponde a la Partition Key en el almacenamiento de telemetría.
    /// </summary>
    public string SectorId { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de la batalla representada como cadena de texto (formato ISO 8601: yyyy-MM-dd).
    /// Facilita la legibilidad del dato LocalDate de Cassandra en el cliente final.
    /// </summary>
    public string Fecha { get; set; } = string.Empty;

    /// <summary>
    /// Marca de tiempo exacta del impacto, incluyendo el desplazamiento de zona horaria (Offset).
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Identificador de la unidad agresora registrada en los logs de combate.
    /// </summary>
    public string NaveAtacante { get; set; } = string.Empty;

    /// <summary>
    /// Identificador de la unidad que recibió el impacto bláster.
    /// </summary>
    public string NaveObjetivo { get; set; } = string.Empty;

    /// <summary>
    /// Valor entero que cuantifica el daño procesado en los sistemas de defensa.
    /// Mapeado desde el campo 'danio_escudos' de la familia de columnas.
    /// </summary>
    public int DanioEscudos { get; set; }
}