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

-- 1. Creación del Keyspace (Base de Datos)

CREATE KEYSPACE IF NOT EXISTS holored 
WITH replication = {'class': 'SimpleStrategy', 'replication_factor': '1'};

-- 2. Selección del Keyspace
USE holored;

-- 3. Creación de la Tabla de Impactos
-- Nota: Se utiliza 'DanoEscudos' sin caracteres especiales (ñ) para evitar errores de sintaxis
CREATE TABLE IF NOT EXISTS impactos (
    SectorId text,
    Fecha date,
    Timestamp timestamp,
    NaveAtacante text,
    NaveObjetivo text,
    DanoEscudos int,
    PRIMARY KEY ((SectorId, Fecha), Timestamp)
) WITH CLUSTERING ORDER BY (Timestamp DESC);