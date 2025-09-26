using PerlaMetro_RouteService.Src.DTOs;

namespace PerlaMetro_RouteService.Src.Interfaces
{
    public interface IRouteRepository
    {
        Task<Models.Route> CreateRouteAsync(Models.Route route);
        Task<Models.Route?> GetRouteByGuidAsync(string guid);
        Task<IEnumerable<Models.Route>> GetAllRoutesAsync();
        Task<Models.Route?> UpdateRouteAsync(
            Models.Route route,
            bool originProvided,
            bool destinationProvided,
            bool stopsProvided
        );
        Task DeleteRouteAsync(string guid);
    }
}
