
# 🌌 API REST: Inteligencia de la Nueva República

**Autores:** Alvaro Naranjo y Adrian Dondaraza

---

## 📋 1. Descripción del Proyecto

Ante el colapso del sistema actual de la HoloRed, este proyecto implementa una arquitectura de **Persistencia Políglota** para el Cuartel General de Inteligencia de la Nueva República. El sistema consiste en una API REST robusta diseñada para procesar el volumen masivo de datos generado por la flota, orquestando tres motores de bases de datos NoSQL especializados para diferentes escenarios en el campo de batalla.

---

## 🛠️ 2. Arquitectura y Tecnologías

* **Framework y Lenguaje:** WebService desarrollado en C#.
* **Arquitectura:** Multicapa con el uso de Controladores tradicionales (estrictamente excluido el uso de Minimal API).
* **Restricción Tecnológica:** Prohibición estricta del uso de motores de bases de datos SQL.

### 🗄️ A. Módulo de Radar y Estado Espacial (Redis)
Motor de base de datos Clave-Valor en memoria RAM, optimizado para tiempos de respuesta de submilisegundos.
* **Gestión de Estado:** Almacena datos bajo el formato `nave:(codigo):estado`.
* **Estados Permitidos:** Solo admite los valores `"patrulla"`, `"hiperespacio"` o `"combate"`.
* **Ciclo de Vida (TTL):** Configurado con una caducidad nativa de **10 minutos**. Si la nave no emite señal, desaparece del radar.
* **Caché (Rutas Rápidas):** Sistema de almacenamiento temporal para cálculos de saltos hiperespaciales recurrentes.

### 📊 B. Módulo de Telemetría de Combate (Cassandra)
Base de datos de Familias de Columnas (P2P) diseñada para operaciones de escritura masiva (*append-only*).
* **Campos Mínimos Obligatorios:** `SectorId` (UUID o String), `Fecha` (Date), `Timestamp`, `NaveAtacante`, `NaveObjetivo` y `DañoEscudos`.
* **Regla de Diseño:** Las consultas (`WHERE`) están estrictamente limitadas por la Clave de Partición, garantizando lecturas sin errores de *Full Scan*.

### 🕸️ C. Módulo de Inteligencia y Sindicatos (Neo4j)
Base de datos Orientada a Grafos para desentrañar alianzas, traiciones y rutas de contrabando.
* **Nodos:** `(Facción)`, `(Planeta)` y `(Espia)`.
* **Relaciones:** `[:CONTROLA]` (de Facción a Planeta), `[:INFILTRADO_EN]` (de Espía a Facción) y `[:SUMINISTRA_ARMAS_A]`.
* **Consultas Cypher:** Diseñadas para saltar al menos dos niveles de profundidad en el grafo.

---

## 🚀 3. Endpoints de la API

| Método | Endpoint | Descripción |
| :--- | :--- | :--- |
| `POST` | `/radar/baliza/{codigo_nave}` | Actualiza el estado de la nave en RAM y renueva su caducidad (TTL) automática. |
| `POST` | `/flota/atraque` | **CRÍTICO:** Solicita permiso de aterrizaje. Proceso concurrente que gestiona hilos obligatoriamente para evitar colisiones. |
| `POST` | `/telemetria/impacto` | Registra impactos de bláster masivos en tiempo real, directamente al motor columnar. |
| `GET` | `/telemetria/historial/{sector}?fecha=YYYY-MM-DD` | Devuelve el registro de batalla nativo de un sector estelar en un día específico. |
| `GET` | `/inteligencia/{faccion}/traidores` | Devuelve espías `[:INFILTRADO_EN]` en tu facción que tengan la relación `[:SUMINISTRA_ARMAS_A]` con una facción rival. |

---

## 🛡️ 4. Concurrencia, Excepciones y Seguridad (Requisitos Críticos)

* **Seguridad Multihilo (Thread-Safety):** Control riguroso de condiciones de carrera (*Race Conditions*). El sistema gestiona bloqueos (*locks*) adecuados para evitar inconsistencias, como asignar la misma bahía de atraque a dos cazas simultáneamente.
* **Manejo de Excepciones Resiliente:** En caso de caída de un nodo bajo fuego enemigo o pérdida de conexión, la API cuenta con bloques `try-catch` robustos. Captura excepciones específicas de los drivers de conexión.
* **Respuestas HTTP Semánticas:** Retorna códigos adecuados como `503 Service Unavailable`. Se evita en todo momento devolver errores `500` genéricos con *StackTrace*.
* **Seguridad de Datos:** Las bases de datos están configuradas de forma segura, impidiendo el acceso a los datos sin los permisos y credenciales correspondientes.

---

## 🐳 5. Despliegue e Infraestructura

* **Repositorio Limpio:** El proyecto se entrega sin binarios (directorios `bin` y `obj` excluidos).
* **Infraestructura Dockerizada:** En la raíz del proyecto se incluye el archivo `docker-compose.yml`, encargado de levantar simultáneamente los contenedores de Redis, Cassandra y Neo4j.
* **Persistencia:** Configuración adecuada de volúmenes para asegurar la persistencia de datos tras la parada o reinicio de los contenedores.