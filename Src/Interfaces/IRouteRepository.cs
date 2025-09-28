using PerlaMetro_RouteService.Src.DTOs;

namespace PerlaMetro_RouteService.Src.Interfaces
{
    public interface IRouteRepository
    {
        /// <summary>
        /// Crea una nueva ruta en la base de datos.
        /// </summary>
        /// <param name="route">Ruta a crear.</param>
        /// <returns>Ruta creada.</returns>
        Task<Models.Route> CreateRouteAsync(Models.Route route);

        /// <summary>
        /// Obtiene una ruta por su GUID.
        /// </summary>
        /// <param name="guid">GUID de la ruta.</param>
        /// <returns>Ruta encontrada o null.</returns>
        Task<Models.Route?> GetRouteByGuidAsync(string guid);

        /// <summary>
        /// Obtiene todas las rutas.
        /// </summary>
        /// <returns>Lista de rutas.</returns>
        Task<IEnumerable<Models.Route>> GetAllRoutesAsync();

        /// <summary>
        /// Actualiza una ruta existente.
        /// </summary>
        /// <param name="route">Ruta a actualizar.</param>
        /// <param name="originProvided">Indica si se proporcionó un origen.</param>
        /// <param name="destinationProvided">Indica si se proporcionó un destino.</param>
        /// <param name="stopsProvided">Indica si se proporcionaron paradas.</param>
        /// <returns>Ruta actualizada o null.</returns>
        Task<Models.Route?> UpdateRouteAsync(
            Models.Route route,
            bool originProvided,
            bool destinationProvided,
            bool stopsProvided
        );

        /// <summary>
        /// Elimina una ruta existente.
        /// </summary>
        /// <param name="guid">GUID de la ruta a eliminar.</param>
        Task DeleteRouteAsync(string guid);
    }
}
