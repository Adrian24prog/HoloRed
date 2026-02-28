namespace HoloRed.Dtos
{
    /// <summary>
    /// Objeto de Transferencia de Datos (DTO) para el registro de impactos de combate.
    /// Se utiliza para transportar la información desde el controlador hasta el motor de Cassandra.
    /// </summary>
    /// <remarks>
    /// Autor: Adrian Dondarza
    /// </remarks>
    public class ImpactoBatallaDto
    {
        /// <summary>
        /// Identificador único del sector estelar donde ocurrió el evento.
        /// Se utiliza como Partition Key en la base de datos.
        /// </summary>
        public string SectorId { get; set; }

        /// <summary>
        /// Identificador o nombre de la nave que realiza el disparo.
        /// </summary>
        public string NaveAtacante { get; set; }

        /// <summary>
        /// Identificador o nombre de la nave que recibe el impacto.
        /// </summary>
        public string NaveObjetivo { get; set; }

        /// <summary>
        /// Cantidad de daño reducido de los escudos deflectores tras el impacto.
        /// </summary>
        public int DañoEscudos { get; set; }

        /// <summary>
        /// Fecha y hora exacta del suceso. 
        /// Crucial para el filtrado temporal en las consultas de telemetría.
        /// </summary>
        public DateTime Fecha { get; set; }
    }
}