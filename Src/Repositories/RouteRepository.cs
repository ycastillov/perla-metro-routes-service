using Neo4j.Driver;
using PerlaMetro_RouteService.Src.Infrastructure.Db;
using PerlaMetro_RouteService.Src.Interfaces;
using PerlaMetro_RouteService.Src.Queries;

namespace PerlaMetro_RouteService.Src.Repositories
{
    public class RouteRepository(ApplicationDbContext context) : IRouteRepository
    {
        private readonly ApplicationDbContext _context = context;

        // ---------------------------
        // Crear ruta
        // ---------------------------
        public async Task<Models.Route> CreateRouteAsync(Models.Route route)
        {
            await using var session = _context.GetSession();

            var stations = new List<string> { route.Origin };
            if (route.Stops != null && route.Stops.Any())
                stations.AddRange(route.Stops);
            stations.Add(route.Destination);

            var tx = await session.BeginTransactionAsync();

            for (int i = 0; i < stations.Count - 1; i++)
            {
                await tx.RunAsync(
                    RouteQueries.CreateRouteSegment,
                    new
                    {
                        from = stations[i],
                        to = stations[i + 1],
                        id = route.Id,
                        start = route.StartTime.ToString(),
                        end = route.EndTime.ToString(),
                        status = route.Status,
                    }
                );
            }

            await tx.CommitAsync();
            return route;
        }

        // ---------------------------
        // Obtener ruta por ID
        // ---------------------------
        public async Task<Models.Route?> GetRouteByGuidAsync(string guid)
        {
            await using var session = _context.GetSession();
            var cursor = await session.RunAsync(RouteQueries.GetRouteById, new { id = guid });
            var record = await cursor.SingleAsync();

            if (record == null)
                return null;

            var stations = record["stations"].As<List<string>>();
            var rel = record["rel"].As<IRelationship>();

            return new Models.Route
            {
                Id = rel.Properties["Id"].As<string>(),
                Origin = stations.First(),
                Destination = stations.Last(),
                Stops = stations.Skip(1).Take(stations.Count - 2).ToList(),
                StartTime = TimeSpan.Parse(rel.Properties["StartTime"].As<string>()),
                EndTime = TimeSpan.Parse(rel.Properties["EndTime"].As<string>()),
                Status = rel.Properties["Status"].As<string>(),
            };
        }

        // ---------------------------
        // Obtener todas las rutas
        // ---------------------------
        public async Task<IEnumerable<Models.Route>> GetAllRoutesAsync()
        {
            await using var session = _context.GetSession();
            var cursor = await session.RunAsync(RouteQueries.GetAllRoutes);
            var records = await cursor.ToListAsync();

            return records.Select(r =>
            {
                var stations = r["stations"].As<List<string>>();
                var rel = r["rel"].As<IRelationship>();

                return new Models.Route
                {
                    Id = r["id"].As<string>(),
                    Origin = stations.First(),
                    Destination = stations.Last(),
                    Stops = stations.Skip(1).Take(stations.Count - 2).ToList(),
                    StartTime = TimeSpan.Parse(rel.Properties["StartTime"].As<string>()),
                    EndTime = TimeSpan.Parse(rel.Properties["EndTime"].As<string>()),
                    Status = rel.Properties["Status"].As<string>(),
                };
            });
        }

        // ---------------------------
        // Actualizar ruta
        // ---------------------------
        public async Task<Models.Route?> UpdateRouteAsync(Models.Route route)
        {
            await using var session = _context.GetSession();

            // 1. Obtener la ruta actual
            var existing = await GetRouteByGuidAsync(route.Id);
            if (existing == null)
                return null;

            // 2. Determinar valores finales
            var finalRoute = new Models.Route
            {
                Id = route.Id,
                Origin = string.IsNullOrEmpty(route.Origin) ? existing.Origin : route.Origin,
                Destination = string.IsNullOrEmpty(route.Destination)
                    ? existing.Destination
                    : route.Destination,
                Stops = route.Stops switch
                {
                    null => existing.Stops, // mantener
                    { Count: 0 } => new List<string>(), // borrar todas
                    _ => route.Stops, // reemplazar
                },
                StartTime = route.StartTime != default ? route.StartTime : existing.StartTime,
                EndTime = route.EndTime != default ? route.EndTime : existing.EndTime,
                Status = !string.IsNullOrEmpty(route.Status) ? route.Status : existing.Status,
            };

            // 3. Si hay que reconstruir (cambio de stops o de estaciones)
            if (
                route.Stops != null
                || !string.IsNullOrEmpty(route.Origin)
                || !string.IsNullOrEmpty(route.Destination)
            )
            {
                var parameters = new
                {
                    id = finalRoute.Id,
                    origin = finalRoute.Origin,
                    destination = finalRoute.Destination,
                    stops = finalRoute.Stops ?? new List<string>(),
                    start = finalRoute.StartTime.ToString(),
                    end = finalRoute.EndTime.ToString(),
                    status = finalRoute.Status,
                };

                var cursor = await session.RunAsync(RouteQueries.RebuildRoute, parameters);
                var record = await cursor.SingleAsync();

                var stations = record["stations"].As<List<string>>();
                var rel = record["rel"].As<IRelationship>();

                return new Models.Route
                {
                    Id = rel.Properties["Id"].As<string>(),
                    Origin = stations.First(),
                    Destination = stations.Last(),
                    Stops = stations.Skip(1).Take(stations.Count - 2).ToList(),
                    StartTime = TimeSpan.Parse(rel.Properties["StartTime"].As<string>()),
                    EndTime = TimeSpan.Parse(rel.Properties["EndTime"].As<string>()),
                    Status = rel.Properties["Status"].As<string>(),
                };
            }
            else
            {
                // 4. Si no hay cambios en stops/origin/destination → solo propiedades
                var parameters = new
                {
                    id = finalRoute.Id,
                    start = finalRoute.StartTime.ToString(),
                    end = finalRoute.EndTime.ToString(),
                    status = finalRoute.Status,
                };

                var cursor = await session.RunAsync(RouteQueries.UpdateRouteProperties, parameters);
                var record = await cursor.SingleAsync();

                var stations = record["stations"].As<List<string>>();
                var rel = record["rel"].As<IRelationship>();

                return new Models.Route
                {
                    Id = rel.Properties["Id"].As<string>(),
                    Origin = stations.FirstOrDefault() ?? string.Empty,
                    Destination = stations.LastOrDefault() ?? string.Empty,
                    Stops = stations.Skip(1).Take(stations.Count - 2).ToList(),
                    StartTime = TimeSpan.Parse(rel.Properties["StartTime"].As<string>()),
                    EndTime = TimeSpan.Parse(rel.Properties["EndTime"].As<string>()),
                    Status = rel.Properties["Status"].As<string>(),
                };
            }
        }

        // ---------------------------
        // Eliminar ruta (soft delete)
        // ---------------------------
        public async Task DeleteRouteAsync(string guid)
        {
            await using var session = _context.GetSession();

            var checkCursor = await session.RunAsync(
                RouteQueries.CheckRouteInactive,
                new { id = guid }
            );
            var checkRecord = await checkCursor.SingleAsync();

            if (checkRecord != null && checkRecord["inactiveCount"].As<int>() > 0)
                throw new Exception("Route is already inactive");

            var cursor = await session.RunAsync(RouteQueries.SoftDeleteRoute, new { id = guid });
            var record = await cursor.SingleAsync();
            if (record == null)
                throw new Exception("Route not found");
        }
    }
}
