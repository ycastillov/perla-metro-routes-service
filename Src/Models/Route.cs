namespace PerlaMetro_RouteService.Src.Models
{
    /// <summary>
    /// Modelo que representa una ruta de metro.
    /// </summary>
    public class Route
    {
        /// <summary>
        /// Identificador único de la ruta.
        /// </summary>
        public required string Id { get; set; } = Guid.NewGuid().ToString();

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
        /// Hora de fin de la ruta.
        /// </summary>
        public required TimeSpan EndTime { get; set; }

        /// <summary>
        /// Lista de paradas intermedias en la ruta.
        /// </summary>
        public List<string>? Stops { get; set; } = null;

        /// <summary>
        /// Estado actual de la ruta (por ejemplo, "Active", "Inactive").
        /// </summary>
        public required string Status { get; set; }
    }
}
