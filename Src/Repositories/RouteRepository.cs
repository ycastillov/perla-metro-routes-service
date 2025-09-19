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
                StartTime = TimeSpan.Parse(node.Properties["StartTime"].As<string>()),
                EndTime = TimeSpan.Parse(node.Properties["EndTime"].As<string>()),
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

        public async Task<IEnumerable<Models.Route>> GetAllRoutesAsync()
        {
            await using var session = _context.GetSession();
            var cursor = await session.RunAsync(RouteQueries.GetAllRoutes);
            var records = await cursor.ToListAsync();

            return records
                .Select(r => new Models.Route
                {
                    Id = r["r"].As<INode>().Properties["Id"].As<string>(),
                    Origin = r["r"].As<INode>().Properties["Origin"].As<string>(),
                    Destination = r["r"].As<INode>().Properties["Destination"].As<string>(),
                    StartTime = TimeSpan.Parse(
                        r["r"].As<INode>().Properties["StartTime"].As<string>()
                    ),
                    EndTime = TimeSpan.Parse(r["r"].As<INode>().Properties["EndTime"].As<string>()),
                    Stops = r["r"].As<INode>().Properties["Stops"].As<List<string>>(),
                    Status = r["r"].As<INode>().Properties["Status"].As<string>(),
                })
                .ToList();
        }

        public async Task<Models.Route?> UpdateRouteAsync(Models.Route route)
        {
            await using var session = _context.GetSession();

            var updates = new List<string>();
            var parameters = new Dictionary<string, object> { { "id", route.Id } };

            if (!string.IsNullOrEmpty(route.Origin))
            {
                updates.Add("r.Origin = $origin");
                parameters["origin"] = route.Origin;
            }

            if (!string.IsNullOrEmpty(route.Destination))
            {
                updates.Add("r.Destination = $destination");
                parameters["destination"] = route.Destination;
            }

            if (route.StartTime != default)
            {
                updates.Add("r.StartTime = $start");
                parameters["start"] = route.StartTime.ToString();
            }

            if (route.EndTime != default)
            {
                updates.Add("r.EndTime = $end");
                parameters["end"] = route.EndTime.ToString();
            }

            if (route.Stops != null && route.Stops.Any())
            {
                updates.Add("r.Stops = $stops");
                parameters["stops"] = route.Stops;
            }

            // Enum a string
            updates.Add("r.Status = $status");
            parameters["status"] = route.Status.ToString();

            var query =
                $@"
        MATCH (r:Route {{ Id: $id }})
        SET {string.Join(", ", updates)}
        RETURN r";

            var cursor = await session.RunAsync(query, parameters);
            var record = await cursor.SingleAsync();
            if (record == null)
                return null;

            var node = record["r"].As<INode>();
            return new Models.Route
            {
                Id = node.Properties["Id"].As<string>(),
                Origin = node.Properties["Origin"].As<string>(),
                Destination = node.Properties["Destination"].As<string>(),
                StartTime = TimeSpan.Parse(node.Properties["StartTime"].As<string>()),
                EndTime = TimeSpan.Parse(node.Properties["EndTime"].As<string>()),
                Stops = node.Properties["Stops"].As<List<string>>(),
                Status = node.Properties["Status"].As<string>(),
            };
        }

        public async Task DeleteRouteAsync(string guid)
        {
            await using var session = _context.GetSession();
            await session.RunAsync(RouteQueries.SoftDeleteRoute, new { id = guid });
        }
    }
}
