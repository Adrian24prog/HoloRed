// SCRIPT PARA CREAR LA BASE DE DATOS DE CASSANDRA

CREATE KEYSPACE IF NOT EXISTS holored 
WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1};

USE holored;

CREATE TABLE IF NOT EXISTS telemetria_combate (
    sector_id text,
    fecha date,
    timestamp timestamp,
    nave_atacante text,
    nave_objetivo text,
    danio_escudos int,
    PRIMARY KEY ((sector_id), fecha, timestamp)
) WITH CLUSTERING ORDER BY (fecha DESC, timestamp DESC) NG ORDER BY (fecha DESC, timestamp DESC);

//Para acceder:
docker exec -it cassandra_telemetria cqlsh -u admin -p RepublicBattle_2026!




// SCRIPT PARA CREAR LA BASE DE DATOS DE NEO4J

// Limpiamos datos anteriores para evitar duplicados
MATCH (n) DETACH DELETE n;

// Creamos las Facciones
CREATE (nr:Faccion {nombre: 'Nueva Republica'})
CREATE (ri:Faccion {nombre: 'Remanente Imperial'})

// Creamos a Kallus y su red de traición
CREATE (k:Espia {nombre: 'Kallus'})

// RELACIÓN OFICIAL: Kallus finge estar en la Nueva República
CREATE (k)-[:INFILTRADO_EN]->(nr)

// RELACIÓN OCULTA: Kallus realmente ayuda al Imperio (esto lo delata)
CREATE (k)-[:SUMINISTRA_ARMAS_A]->(ri)
CREATE (k)-[:INFORMA_MOVIMIENTOS_A]->(ri)

// Creamos un espía LEAL para comparar (Andor no debería salir)
CREATE (a:Espia {nombre: 'Andor'})
CREATE (a)-[:INFILTRADO_EN]->(ri)
CREATE (a)-[:SABOTEA_OPERACIONES_DE]->(ri)USTERING ORDER BY (Timestamp DESC);

//Para acceder:
docker exec -it neo4j_espionaje cypher-shell -u neo4j -p RepublicSpies_2026!



// Redis funciona como un almacén de Llave-Valor (Key-Value Store).
Comprobacion de 10 minuntos TTL:
docker exec -it redis_radar redis-cli -a RepublicRadar_2026!
2) KEYS * (Para ver el nombre de la clave, ej: "nave:XWING-01").
3) TTL "nave:XWING-01" (Debe devolver un número menor a 600 segundos

//Para acceder:
docker exec -it redis_radar redis-cli -a RepublicRadar_2026!