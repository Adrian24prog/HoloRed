using Neo4j.Driver;
using HoloRed.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoloRed.Infrastructure.Neo4j;

/// <summary>
/// Implementación del repositorio de inteligencia galáctica utilizando Neo4j.
/// Diseñado para desentrañar traiciones mediante el análisis de conexiones profundas
/// en el grafo de la HoloRed.
/// </summary>
/// <remarks>
/// <author>Álvaro Naranjo y Adrian Dondarza</author>
/// <date>03/03/2026</date>
/// </remarks>
public class Neo4jEspionajeRepository : IEspionajeRepository
{
    private readonly IDriver _driver;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio inyectando el driver oficial de Neo4j.
    /// </summary>
    /// <param name="driver">Instancia de IDriver configurada para la conexión con el motor de grafos.</param>
    public Neo4jEspionajeRepository(IDriver driver)
    {
        _driver = driver;
    }

    /// <summary>
    /// Ejecuta una consulta Cypher de múltiples saltos para detectar agentes comprometidos.
    /// Identifica espías que están infiltrados en una facción mientras suministran armamento a la facción rival.
    /// </summary>
    /// <param name="origen">Nombre de la facción donde se sospecha la infiltración.</param>
    /// <param name="destino">Nombre de la facción que recibe el suministro ilegal de armas.</param>
    /// <returns>Colección de nombres de los agentes detectados por la red de inteligencia.</returns>
    public async Task<IEnumerable<string>> ObtenerInfiltradosAsync(string origen, string destino)
    {
        // Consulta Cypher parametrizada que salta dos niveles de profundidad en el grafo
        // Requisito: (Facción)<-[:INFILTRADO_EN]-(Espía)-[:SUMINISTRA_ARMAS_A]->(Facción rival)
        var query = @"
            MATCH (f1:Faccion {nombre: $origen})<-[:INFILTRADO_EN]-(e:Espia)-[:SUMINISTRA_ARMAS_A]->(f2:Faccion {nombre: $destino})
            RETURN e.nombre AS info";

        // Gestión de sesión asíncrona para maximizar el rendimiento en entornos concurrentes
        var session = _driver.AsyncSession();
        try
        {
            // Ejecución de la transacción en modo lectura para optimizar recursos del clúster
            return await session.ExecuteReadAsync(async tx => {
                var result = await tx.RunAsync(query, new { origen, destino });
                var records = await result.ToListAsync();

                // Proyección de los resultados del grafo a tipos de datos compatibles con C#
                return records.Select(r => r["info"].As<string>());
            });
        }
        catch (ServiceUnavailableException ex)
        {
            // Requisito 4: Captura específica de errores de conexión con el nodo de Neo4j
            throw new System.Exception("Error de comunicación con el motor de inteligencia de grafos.", ex);
        }
        finally
        {
            // Cierre preventivo de la sesión para liberar el pool de conexiones del driver
            await session.CloseAsync();
        }
    }
}