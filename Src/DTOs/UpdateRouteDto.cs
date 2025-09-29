namespace PerlaMetro_RouteService.Src.DTOs
{
    /// <summary>
    /// Data Transfer Object para actualizar una ruta.
    /// </summary>
    public class UpdateRouteDto
    {
        /// <summary>
        /// Estación de origen de la ruta.
        /// </summary>
        public string? Origin { get; set; }

        /// <summary>
        /// Estación de destino de la ruta.
        /// </summary>
        public string? Destination { get; set; }

        /// <summary>
        /// Hora de inicio de la ruta.
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// Hora de finalización de la ruta.
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Paradas intermedias de la ruta.
        /// </summary>
        public List<string>? Stops { get; set; }
    }
}
