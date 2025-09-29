namespace PerlaMetro_RouteService.Src.DTOs
{
    /// <summary>
    /// Data Transfer Object para crear una nueva ruta.
    /// </summary>
    public class CreateRouteDto
    {
        /// <summary>
        /// Estación de origen de la ruta.
        /// </summary>
        public required string Origin { get; set; }

        /// <summary>
        /// Estación de destino de la ruta.
        /// </summary>
        public required string Destination { get; set; }

        /// <summary>
        /// Hora de inicio de la ruta.
        /// </summary>
        public required TimeSpan StartTime { get; set; }

        /// <summary>
        /// Hora de finalización de la ruta.
        /// </summary>
        public required TimeSpan EndTime { get; set; }

        /// <summary>
        /// Paradas intermedias de la ruta.
        /// </summary>
        public List<string>? Stops { get; set; }

        /// <summary>
        /// Estado de la ruta (por ejemplo, "Active", "Inactive").
        /// /// </summary>
        public string Status { get; set; } = "Active";
    }
}
