CREATE TABLE telemetria_combate (
    sector_id text,
    fecha date,
    timestamp timestamp,
    nave_atacante text,
    nave_objetivo text,
    danio_escudos int,
    PRIMARY KEY ((sector_id), fecha, timestamp)
) WITH CLUSTERING ORDER BY (fecha DESC, timestamp DESC);