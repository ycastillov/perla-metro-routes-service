using System.Text;
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

            var query =
                @"
                MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
                WHERE ALL(r IN rels WHERE r.Id = $id)
                WITH [n IN nodes(path) | n.name] AS stations, rels[0] AS rel, length(path) AS len
                ORDER BY len DESC
                LIMIT 1
                RETURN stations, rel
            ";

            var cursor = await session.RunAsync(query, new { id = guid });
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
                var query =
                    @"
                MERGE (a:Station { name: $from })
                MERGE (b:Station { name: $to })
                CREATE (a)-[:ROUTE {
                    Id: $id,
                    StartTime: $start,
                    EndTime: $end,
                    Status: $status
                }]->(b)";

                await tx.RunAsync(
                    query,
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

            var query =
                @"
                MATCH path=(origin:Station)-[r:ROUTE*]->(dest:Station)
                WITH r[0] AS rel, [n IN nodes(path) | n.name] AS stations
                RETURN DISTINCT rel.Id AS id, stations, rel";

            var cursor = await session.RunAsync(query);
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

            // Si se mandan paradas nuevas -> reconstruir la ruta
            if (route.Stops != null && route.Stops.Any())
            {
                var parameters = new
                {
                    id = route.Id,
                    origin = route.Origin,
                    destination = route.Destination,
                    stops = route.Stops,
                    start = route.StartTime.ToString(),
                    end = route.EndTime.ToString(),
                    status = route.Status.ToString(),
                };

                var query =
                    @"
                    // 1. Eliminar las relaciones existentes de esta ruta
                    MATCH path=(o:Station)-[rels:ROUTE*]->(d:Station)
                    WHERE ALL(r IN rels WHERE r.Id = $id)
                    WITH rels
                    FOREACH (r IN rels | DELETE r)

                    // 2. Crear estaciones de origen y destino
                    MERGE (s0:Station { name: $origin })
                    MERGE (sLast:Station { name: $destination })

                    // 3. Construir lista completa de estaciones
                    WITH [s0] + [st IN $stops | (MERGE (s:Station { name: st }) RETURN s)] + [sLast] AS stations

                    // 4. Crear relaciones ROUTE entre estaciones consecutivas
                    UNWIND range(0, size(stations)-2) AS i
                    WITH stations[i] AS a, stations[i+1] AS b
                    MERGE (a)-[r:ROUTE { Id: $id }]->(b)
                    SET r.StartTime = $start,
                        r.EndTime = $end,
                        r.Status = $status
                    RETURN r
                ";

                var cursor = await session.RunAsync(query, parameters);
                var record = await cursor.SingleAsync();

                if (record == null)
                    return null;

                var rel = record["r"].As<IRelationship>();
                return new Models.Route
                {
                    Id = rel.Properties["Id"].As<string>(),
                    Origin = route.Origin,
                    Destination = route.Destination,
                    Stops = route.Stops,
                    StartTime = TimeSpan.Parse(rel.Properties["StartTime"].As<string>()),
                    EndTime = TimeSpan.Parse(rel.Properties["EndTime"].As<string>()),
                    Status = rel.Properties["Status"].As<string>(),
                };
            }
            else
            {
                // Solo actualizar propiedades de las relaciones
                var parameters = new
                {
                    id = route.Id,
                    start = route.StartTime != default ? route.StartTime.ToString() : null,
                    end = route.EndTime != default ? route.EndTime.ToString() : null,
                    status = !string.IsNullOrEmpty(route.Status) ? route.Status.ToString() : null,
                };

                var query =
                    @"
                    MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
                    WHERE ALL(r IN rels WHERE r.Id = $id)
                    WITH rels, [n IN nodes(path) | n.name] AS stations, length(path) AS len
                    ORDER BY len DESC
                    LIMIT 1
                    FOREACH (r IN rels |
                        SET r.StartTime = coalesce($start, r.StartTime),
                            r.EndTime   = coalesce($end, r.EndTime),
                            r.Status    = coalesce($status, r.Status)
                    )
                    RETURN stations, rels[0] AS rel
                ";

                var cursor = await session.RunAsync(query, parameters);
                var record = await cursor.SingleAsync();

                if (record == null)
                    return null;

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

        public async Task DeleteRouteAsync(string guid)
        {
            await using var session = _context.GetSession();

            var checkQuery =
                @"
                MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
                WHERE ALL(r IN rels WHERE r.Id = $id)
                WITH rels, [r IN rels WHERE r.Status = 'Inactive'] AS inactiveRels
                RETURN size(inactiveRels) AS inactiveCount
                LIMIT 1
            ";

            var checkCursor = await session.RunAsync(checkQuery, new { id = guid });
            var checkRecord = await checkCursor.SingleAsync();

            if (checkRecord != null && checkRecord["inactiveCount"].As<int>() > 0)
                throw new Exception("Route is already inactive");

            var query =
                @"
                MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
                WHERE ALL(r IN rels WHERE r.Id = $id)
                WITH rels, [n IN nodes(path) | n.name] AS stations, length(path) AS len
                ORDER BY len DESC
                LIMIT 1
                FOREACH (r IN rels | SET r.Status = 'Inactive')
                RETURN stations, rels[0] AS rel
            ";

            var cursor = await session.RunAsync(query, new { id = guid });
            var record = await cursor.SingleAsync();

            if (record == null)
                throw new Exception("Route not found");
        }
    }
}
