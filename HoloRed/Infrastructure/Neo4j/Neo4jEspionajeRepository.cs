using Neo4j.Driver;
using HoloRed.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoloRed.Infrastructure.Neo4j;

/// <summary>
/// Implementación de repositorio para consultas de espionaje en Neo4j.
/// </summary>
/// <remarks>Autor:Álvaro Naranjo y  Adrian Dondarza</remarks>
public class Neo4jEspionajeRepository : IEspionajeRepository
{
    private readonly IDriver _driver;

    /// <summary>
    /// Constructor que inyecta el driver de Neo4j.
    /// </summary>
    /// <param name="driver">Instancia de IDriver para conectarse a la base de grafos.</param>
    public Neo4jEspionajeRepository(IDriver driver)
    {
        _driver = driver;
    }

    /// <summary>
    /// Obtiene los espías infiltrados que cumplen la relación:
    /// origen &lt;- INFILTRADO_EN - Espía - SUMINISTRA_ARMAS_A -&gt; destino
    /// </summary>
    /// <param name="origen">Nombre de la facción de origen del espía.</param>
    /// <param name="destino">Nombre de la facción destino donde está infiltrado.</param>
    /// <returns>Lista de nombres de espías que coinciden con la ruta en el grafo.</returns>
    public async Task<IEnumerable<string>> ObtenerInfiltradosAsync(string origen, string destino)
    {
        // Consulta Cypher parametrizada para evitar inyección
        var query = @"
            MATCH (f1:Faccion {nombre: $origen})<-[:INFILTRADO_EN]-(e:Espia)-[:SUMINISTRA_ARMAS_A]->(f2:Faccion {nombre: $destino})
            RETURN e.nombre AS info";

        // Creamos una sesión asincrónica con Neo4j
        var session = _driver.AsyncSession();
        try
        {
            // Ejecutamos la consulta en modo lectura
            return await session.ExecuteReadAsync(async tx => {
                var result = await tx.RunAsync(query, new { origen, destino });
                var records = await result.ToListAsync();

                // Extraemos y retornamos solo los nombres de los espías
                return records.Select(r => r["info"].As<string>());
            });
        }
        finally
        {
            // Cerramos la sesión para liberar recursos
            await session.CloseAsync();
        }
    }
}