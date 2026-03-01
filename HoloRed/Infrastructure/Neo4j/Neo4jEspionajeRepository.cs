using Neo4j.Driver;
using HoloRed.Domain.Interfaces;

namespace HoloRed.Infrastructure.Neo4j;

public class Neo4jEspionajeRepository : IEspionajeRepository
{
    private readonly IDriver _driver;

    public Neo4jEspionajeRepository(IDriver driver)
    {
        _driver = driver;
    }

    public async Task<IEnumerable<string>> ObtenerInfiltradosAsync(string origen, string destino)
    {
        // Consulta 
        var query = @"
                MATCH (f1:Faccion {nombre: $origen})<-[:INFILTRADO_EN]-(e:Espia)-[:SUMINISTRA_ARMAS_A]->(f2:Faccion {nombre: $destino})
                RETURN e.nombre AS info";

        var session = _driver.AsyncSession();
        try
        {
            return await session.ExecuteReadAsync(async tx => {
                var result = await tx.RunAsync(query, new { origen, destino });
                var records = await result.ToListAsync();
                return records.Select(r => r["info"].As<string>());
            });
        }
        finally
        {
            await session.CloseAsync();
        }
    }
}