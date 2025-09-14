using Neo4j.Driver;
using PerlaMetro_RouteService.Src.Infrastructure.Db;
using PerlaMetro_RouteService.Src.Interfaces;
using PerlaMetro_RouteService.Src.Models;
using PerlaMetro_RouteService.Src.Queries;

namespace PerlaMetro_RouteService.Src.Repositories
{
    public class RouteRepository(ApplicationDbContext context) : IRouteRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<Models.Route?> GetRouteByGuidAsync(string guid)
        {
            await using var session = _context.GetSession();
            var cursor = await session.RunAsync(RouteQueries.GetRouteByGuid, new { id = guid });
            var record = await cursor.SingleAsync();

            if (record == null)
                return null;

            var node = record["r"].As<INode>();
            return new Models.Route
            {
                Id = node.Properties["Id"].As<string>(),
                Origin = node.Properties["Origin"].As<string>(),
                Destination = node.Properties["Destination"].As<string>(),
                StartTime = node.Properties["StartTime"].As<TimeSpan>(),
                EndTime = node.Properties["EndTime"].As<TimeSpan>(),
                Stops = node.Properties["Stops"].As<List<string>>(),
                Status = node.Properties["Status"].As<string>(),
            };
        }

        public async Task CreateRouteAsync(Models.Route route)
        {
            await using var session = _context.GetSession();
            await session.RunAsync(
                RouteQueries.CreateRoute,
                new
                {
                    id = route.Id,
                    origin = route.Origin,
                    destination = route.Destination,
                    start = route.StartTime,
                    end = route.EndTime,
                    stops = route.Stops,
                    status = route.Status.ToString(),
                }
            );
        }
    }
}
