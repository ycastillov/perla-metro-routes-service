namespace PerlaMetro_RouteService.Src.Queries
{
    public static class RouteQueries
    {
        public const string GetRouteByGuid =
            @"
            MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
            WHERE ALL(r IN rels WHERE r.Id = $id)
            WITH [n IN nodes(path) | n.name] AS stations, rels[0] AS rel, length(path) AS len
            ORDER BY len DESC
            LIMIT 1
            RETURN stations, rel
        ";

        // public const string CreateSegment =
        //     @"
        //     MERGE (a:Station { name: $from })
        //     MERGE (b:Station { name: $to })
        //     CREATE (a)-[:ROUTE {
        //         Id: $id,
        //         StartTime: $start,
        //         EndTime: $end,
        //         Status: $status
        //     }]->(b)
        // ";

        public const string GetAllRoutes =
            @"
            MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
            WITH rels, [n IN nodes(path) | n.name] AS stations, length(path) AS len
            ORDER BY len DESC
            LIMIT 1
            WITH rels, stations
            RETURN DISTINCT rels[0].Id AS id, stations, rels[0] AS rel
        ";

        public const string DeleteRoute =
            @"
            MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
            WHERE ALL(r IN rels WHERE r.Id = $id)
            WITH rels, [n IN nodes(path) | n.name] AS stations, length(path) AS len
            ORDER BY len DESC
            LIMIT 1
            FOREACH (r IN rels | SET r.Status = 'Inactive')
            RETURN stations, rels[0] AS rel
        ";

        public const string CheckInactive =
            @"
            MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
            WHERE ALL(r IN rels WHERE r.Id = $id)
            WITH rels, [r IN rels WHERE r.Status = 'Inactive'] AS inactiveRels
            RETURN size(inactiveRels) AS inactiveCount
            LIMIT 1
        ";

        public const string UpdateRouteProperties =
            @"
            MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
            WHERE ALL(r IN rels WHERE r.Id = $id)
            WITH rels, [n IN nodes(path) | n.name] AS stations, length(path) AS len
            ORDER BY len DESC
            LIMIT 1
            FOREACH (r IN rels |
                SET r.StartTime = $start,
                    r.EndTime   = $end,
                    r.Status    = $status
            )
            RETURN stations, rels[0] AS rel
        ";

        public const string DeleteRouteRelationships =
            @"
            MATCH path=(o:Station)-[rels:ROUTE*]->(d:Station)
            WHERE ALL(r IN rels WHERE r.Id = $id)
            WITH rels
            FOREACH (r IN rels | DELETE r)
        ";

        public const string CreateSegment =
            @"
            MERGE (a:Station { name: $from })
            MERGE (b:Station { name: $to })
            MERGE (a)-[r:ROUTE { Id: $id }]->(b)
            SET r.StartTime = $start, r.EndTime = $end, r.Status = $status
            RETURN r
        ";  

        // Lectura: ruta principal por id (longest path)
        public const string GetLongestRouteById =
            @"
            MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
            WHERE ALL(r IN rels WHERE r.Id = $id)
            WITH [n IN nodes(path) | n.name] AS stations, rels[0] AS rel, length(path) AS len
            ORDER BY len DESC
            LIMIT 1
            RETURN stations, rel
        ";
        // @"
        // // Delete old relationships for this route
        // MATCH path=(o:Station)-[rels:ROUTE*]->(d:Station)
        // WHERE ALL(r IN rels WHERE r.Id = $id)
        // WITH rels
        // FOREACH (r IN rels | DELETE r)

        // // Always create origin and destination nodes
        // MERGE (s0:Station { name: $origin })
        // MERGE (sLast:Station { name: $destination })

        // // Handle stops based on whether the list is empty
        // WITH s0, sLast,
        //     CASE 
        //         WHEN size($stops) = 0 
        //         THEN []
        //         ELSE [stop IN $stops WHERE stop <> $origin AND stop <> $destination]
        //     END AS filteredStops

        // // Create stop nodes or handle empty case
        // WITH s0, sLast, filteredStops
        // WITH s0, sLast,
        //     CASE 
        //         WHEN size(filteredStops) = 0 
        //         THEN []
        //         ELSE apoc.coll.toSet(filteredStops)
        //     END AS uniqueStops
        // UNWIND (
        //     CASE 
        //         WHEN size(uniqueStops) = 0 
        //         THEN [[s0, sLast]] 
        //         ELSE [uniqueStops]
        //     END
        // ) AS stops
        // WITH s0, sLast, stops
        // CALL {
        //     WITH stops
        //     UNWIND stops AS stopName
        //     WITH stopName
        //     WHERE stopName IS NOT NULL
        //     MERGE (s:Station { name: stopName })
        //     RETURN collect(s) AS stopNodes
        // }

        // // Build route - direct or through stops
        // WITH s0, sLast,
        //     CASE 
        //         WHEN size(stopNodes) = 0 
        //         THEN [s0, sLast]
        //         ELSE [s0] + stopNodes + [sLast]
        //     END AS stations

        // // Create relationships
        // UNWIND range(0, size(stations)-2) AS i
        // WITH stations, stations[i] AS a, stations[i+1] AS b
        // WHERE a <> b
        // MERGE (a)-[r:ROUTE { Id: $id }]->(b)
        // SET r.StartTime = $start,
        //     r.EndTime = $end,
        //     r.Status = $status
        // WITH stations, collect(r) AS rels
        // RETURN stations, rels[0] AS rel
        // ";
    }
}
