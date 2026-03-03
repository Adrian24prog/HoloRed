namespace HoloRed.Domain.Entities;

/// <summary>
/// Representa un nodo de tipo Espía dentro del ecosistema de inteligencia de Neo4j.
/// Esta entidad es fundamental para trazar las relaciones de infiltración y 
/// suministro de armamento entre facciones rivales.
/// </summary>
/// <remarks>
/// <author>Adrian Dondarza</author>
/// <date>02/03/2026</date>
/// </remarks>
public class Espia
{
    /// <summary>
    /// Nombre operativo del agente (ej. Cassian Andor).
    /// Se utiliza como identificador visual en las consultas Cypher.
    /// </summary>
    public string Nombre { get; set; }

    /// <summary>
    /// Código de encriptación o alias asignado por la inteligencia (ej. Fulcrum).
    /// Permite la trazabilidad segura del agente en la red de grafos.
    /// </summary>
    public string Codigo { get; set; }
}