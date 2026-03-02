namespace HoloRed.Dtos;

public class RegistroCombateDto
{
    public string SectorId { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty; // La enviamos como string "yyyy-MM-dd"
    public DateTimeOffset Timestamp { get; set; }
    public string NaveAtacante { get; set; } = string.Empty;
    public string NaveObjetivo { get; set; } = string.Empty;
    public int DanioEscudos { get; set; }
}