namespace HoloRed.Domain.Entities;

/// <summary>
/// Representa una organización política o militar dentro del motor de grafos Neo4j.
/// Actúa como un nodo central para establecer relaciones de control territorial 
/// e identificación de infiltraciones en el Módulo de Inteligencia.
/// </summary>
/// <remarks>
/// <author>Adrian Dondarza</author>
/// <date>03/03/2026</date>
/// </remarks>
public class Faccion
{
    /// <summary>
    /// Nombre distintivo de la organización (ej. Imperio Galáctico, Alianza Rebelde).
    /// Se utiliza como identificador clave en las consultas Cypher para trazar rutas de espionaje.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
}