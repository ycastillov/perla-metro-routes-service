using Neo4j.Driver;
using PerlaMetro_RouteService.Src.Infrastructure.Db;
using PerlaMetro_RouteService.Src.Interfaces;
using PerlaMetro_RouteService.Src.Queries;

namespace PerlaMetro_RouteService.Src.Repositories
{
    /// <summary>
    /// Repositorio para gestionar rutas en la base de datos Neo4j.
    /// </summary>
    /// <param name="context">Contexto de la base de datos.</param>
    public class RouteRepository(ApplicationDbContext context) : IRouteRepository
    {
        private readonly ApplicationDbContext _context = context;

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

        public async Task<Models.Route?> UpdateRouteAsync(
            Models.Route route,
            bool originProvided,
            bool destinationProvided,
            bool stopsProvided
        )
        {
            await using var session = _context.GetSession();

            var existing = await GetRouteByGuidAsync(route.Id);
            if (existing == null)
                return null;

            // Decidir qué query ejecutar
            if (stopsProvided)
            {
                // Reemplazar paradas (incluso [] = borrar todas)
                var parameters = new
                {
                    id = route.Id,
                    origin = route.Origin,
                    destination = route.Destination,
                    stops = route.Stops ?? new List<string>(),
                    start = route.StartTime.ToString(),
                    end = route.EndTime.ToString(),
                };
                var cursor = await session.RunAsync(RouteQueries.UpdateRouteStops, parameters);
                var record = await cursor.SingleAsync();
                return MapRouteRecord(record);
            }
            else if (originProvided || destinationProvided)
            {
                // Cambiar origen/destino manteniendo paradas actuales
                var parameters = new
                {
                    id = route.Id,
                    origin = route.Origin,
                    destination = route.Destination,
                    start = route.StartTime.ToString(),
                    end = route.EndTime.ToString(),
                };
                var cursor = await session.RunAsync(RouteQueries.UpdateRouteEndpoints, parameters);
                var record = await cursor.SingleAsync();
                return MapRouteRecord(record);
            }
            else
            {
                // Solo actualizar propiedades
                var parameters = new
                {
                    id = route.Id,
                    start = route.StartTime.ToString(),
                    end = route.EndTime.ToString(),
                };
                var cursor = await session.RunAsync(RouteQueries.UpdateRouteProperties, parameters);
                var record = await cursor.SingleAsync();
                return MapRouteRecord(record);
            }
        }

        /// <summary>
        /// Mapea un registro de ruta a un objeto de modelo de ruta.
        /// </summary>
        /// <param name="record">Registro de ruta.</param>
        /// <returns>Objeto de modelo de ruta.</returns>
        private Models.Route? MapRouteRecord(IRecord record)
        {
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
