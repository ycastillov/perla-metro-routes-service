using Neo4j.Driver;
using PerlaMetro_RouteService.Src.Infrastructure.Db;
using PerlaMetro_RouteService.Src.Interfaces;
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

        public async Task<Models.Route> CreateRouteAsync(Models.Route route)
        {
            await using var session = _context.GetSession();

            // Construir la lista completa de estaciones: origen + paradas + destino
            var stations = new List<string> { route.Origin };
            if (route.Stops != null && route.Stops.Any())
                stations.AddRange(route.Stops);
            stations.Add(route.Destination);

            var tx = await session.BeginTransactionAsync();

            for (int i = 0; i < stations.Count - 1; i++)
            {
                await tx.RunAsync(
                    RouteQueries.CreateSegment,
                    new
                    {
                        from = stations[i],
                        to = stations[i + 1],
                        id = route.Id,
                        start = route.StartTime.ToString(),
                        end = route.EndTime.ToString(),
                        status = route.Status.ToString(),
                    }
                );
            }

            await tx.CommitAsync();
            return route;
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

        public async Task<Models.Route?> UpdateRouteAsync(Models.Route route)
        {
            await using var session = _context.GetSession();

            // 1) leer ruta existente
            var existing = await GetRouteByGuidAsync(route.Id);
            if (existing == null) return null;

            // 3) determinar origin/destination finales (mantener existentes si no se envían)
            var finalOrigin = string.IsNullOrWhiteSpace(route.Origin) ? existing.Origin : route.Origin.Trim();
            var finalDestination = string.IsNullOrWhiteSpace(route.Destination) ? existing.Destination : route.Destination.Trim();

            // 2) calcular finalStops respetando null vs empty:
            //    - route.Stops == null => conservar existing.Stops
            //    - route.Stops == []   => quitar todas las paradas
            //    - route.Stops != null && >0 => reemplazar por las enviadas
            var stopsProvided = route.Stops != null; // explicit instruction
            var finalStops = stopsProvided
                ? route.Stops.Where(s => !string.IsNullOrWhiteSpace(s))
                             .Select(s => s.Trim())
                             .Where(s => s != finalOrigin && s != finalDestination)
                             .ToList()
                : (existing.Stops ?? new List<string>());

            // 4) sanitizar finalStops: quitar vacíos y evitar que coincidan con origin/destination
            finalStops = finalStops
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Where(s => s != finalOrigin && s != finalDestination)
                .ToList();

            // 5) construir stationNames en el orden exacto a recrear
            var stationNames = new List<string> { finalOrigin };
            stationNames.AddRange(finalStops);
            stationNames.Add(finalDestination);

            // 6) decidir si debemos reconstruir estructura:
            bool originChanged = route.Origin != null && route.Origin != existing.Origin;
            bool destinationChanged = route.Destination != null && route.Destination != existing.Destination;

            bool shouldRebuild = stopsProvided || originChanged || destinationChanged;

            // 7) si rebuild -> borrar relaciones y recrear segmentos en TX; devolvemos finalRoute construido en C#
            if (shouldRebuild)
            {
                var tx = await session.BeginTransactionAsync();
                try
                {
                    // borrar relaciones existentes para el id (simple y seguro)
                    await tx.RunAsync(RouteQueries.DeleteRouteRelationships, new { id = route.Id });

                    // crear cada segmento (pares consecutivos)
                    for (int i = 0; i < stationNames.Count - 1; i++)
                    {
                        await tx.RunAsync(RouteQueries.CreateSegment, new
                        {
                            from = stationNames[i],
                            to = stationNames[i + 1],
                            id = route.Id,
                            start = (route.StartTime != default ? route.StartTime : existing.StartTime).ToString(),
                            end = (route.EndTime != default ? route.EndTime : existing.EndTime).ToString(),
                            status = !string.IsNullOrEmpty(route.Status) ? route.Status : existing.Status
                        });
                    }

                    await tx.CommitAsync();

                    // Devolver el objeto resultante (sabemos exactamente lo que creamos)
                    return new Models.Route
                    {
                        Id = route.Id,
                        Origin = finalOrigin,
                        Destination = finalDestination,
                        Stops = finalStops,
                        StartTime = (route.StartTime != default ? route.StartTime : existing.StartTime),
                        EndTime = (route.EndTime != default ? route.EndTime : existing.EndTime),
                        Status = !string.IsNullOrEmpty(route.Status) ? route.Status : existing.Status
                    };
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }

            // 8) si no rebuild -> solo actualizar propiedades de las relaciones existentes
            var parameters = new
            {
                id = route.Id,
                start = (route.StartTime != default ? route.StartTime : existing.StartTime).ToString(),
                end = (route.EndTime != default ? route.EndTime : existing.EndTime).ToString(),
                status = !string.IsNullOrEmpty(route.Status) ? route.Status : existing.Status
            };

            var cursor = await session.RunAsync(RouteQueries.UpdateRouteProperties, parameters);
            var record = await cursor.SingleAsync(); // LIMIT 1 en la query evita multiples rows
            if (record == null) return null;

            var stations = record["stations"].As<List<string>>();
            var rel = record["rel"].As<IRelationship>();

            return new Models.Route
            {
                Id = rel.Properties["Id"].As<string>(),
                Origin = stations.FirstOrDefault() ?? existing.Origin,
                Destination = stations.LastOrDefault() ?? existing.Destination,
                Stops = stations.Count > 2 ? stations.Skip(1).Take(stations.Count - 2).ToList() : new List<string>(),
                StartTime = TimeSpan.Parse(rel.Properties["StartTime"].As<string>()),
                EndTime = TimeSpan.Parse(rel.Properties["EndTime"].As<string>()),
                Status = rel.Properties["Status"].As<string>(),
            };
        }


        public async Task DeleteRouteAsync(string guid)
        {
            await using var session = _context.GetSession();

            var checkCursor = await session.RunAsync(RouteQueries.CheckInactive, new { id = guid });
            var checkRecord = await checkCursor.SingleAsync();

            if (checkRecord != null && checkRecord["inactiveCount"].As<int>() > 0)
                throw new Exception("Route is already inactive");

            var cursor = await session.RunAsync(RouteQueries.DeleteRoute, new { id = guid });
            var record = await cursor.SingleAsync();

            if (record == null)
                throw new Exception("Route not found");
        }
    }
}
