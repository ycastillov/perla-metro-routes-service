namespace PerlaMetro_RouteService.Src.DTOs
{
    /// <summary>
    /// Data Transfer Object para una ruta inactiva.
    /// </summary>
    public class InactiveRouteDto
    {
        /// <summary>
        /// Identificador único de la ruta.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Estación de origen de la ruta.
        /// </summary>
        public string Origin { get; set; } = string.Empty;

        /// <summary>
        /// Estación de destino de la ruta.
        /// </summary>
        public string Destination { get; set; } = string.Empty;

        /// <summary>
        /// Hora de inicio de la ruta.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Hora de finalización de la ruta.
        /// </summary>
        public TimeSpan EndTime { get; set; }
    }
}
