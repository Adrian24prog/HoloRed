namespace HoloRed.Dtos;

/// <summary>
/// Objeto de transferencia para la actualización de la baliza de radar.
/// </summary>
/// <author>Álvaro Naranjo</author>
/// <date>28/02/2026</date>
public class BalizaUpdateDto
{
    /// <summary>
    /// El nuevo estado de la nave: "patrulla", "hiperespacio" o "combate".
    /// </summary>
    public string Estado { get; set; } = string.Empty;
}
