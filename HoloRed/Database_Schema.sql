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
) WITH CLUSTERING ORDER BY (fecha DESC, timestamp DESC);NG ORDER BY (fecha DESC, timestamp DESC);