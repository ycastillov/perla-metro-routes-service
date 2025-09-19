using PerlaMetro_RouteService.Src.DTOs;

namespace PerlaMetro_RouteService.Src.Interfaces
{
    public interface IRouteRepository
    {
        Task<Models.Route?> GetRouteByGuidAsync(string guid);

        Task<IEnumerable<Models.Route>> GetAllRoutesAsync();
        Task CreateRouteAsync(Models.Route route);
        Task<Models.Route?> UpdateRouteAsync(Models.Route route);
        Task DeleteRouteAsync(string guid);
    }
}
