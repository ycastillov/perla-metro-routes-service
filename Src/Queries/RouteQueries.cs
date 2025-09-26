namespace PerlaMetro_RouteService.Src.Queries
{
    public static class RouteQueries
    {
        // Crear un segmento de ruta entre dos estaciones consecutivas
        public const string CreateRouteSegment =
            @"
            MERGE (a:Station { name: $from })
            MERGE (b:Station { name: $to })
            CREATE (a)-[:ROUTE {
                Id: $id,
                StartTime: $start,
                EndTime: $end,
                Status: $status
            }]->(b)
        ";

        // Obtener ruta por GUID (la más larga = ruta principal)
        public const string GetRouteById =
            @"
            MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
            WHERE ALL(r IN rels WHERE r.Id = $id)
            WITH [n IN nodes(path) | n.name] AS stations, rels[0] AS rel, length(path) AS len
            ORDER BY len DESC
            LIMIT 1
            RETURN stations, rel
        ";

        // Obtener todas las rutas (agrupadas por Id)
        public const string GetAllRoutes =
            @"
            MATCH path=(origin:Station)-[r:ROUTE*]->(dest:Station)
            WITH r[0] AS rel, [n IN nodes(path) | n.name] AS stations, length(path) AS len
            ORDER BY len DESC
            RETURN DISTINCT rel.Id AS id, stations, rel
        ";

        // Reconstruir ruta (cuando cambian stops, origen o destino)
        public const string RebuildRoute =
            @"
            // 1. Eliminar relaciones antiguas
            MATCH path=(o:Station)-[rels:ROUTE*]->(d:Station)
            WHERE ALL(r IN rels WHERE r.Id = $id)
            WITH rels
            FOREACH (r IN rels | DELETE r)

            // 2. Crear estaciones de origen y destino
            MERGE (s0:Station { name: $origin })
            MERGE (sLast:Station { name: $destination })

            // 3. Manejo de paradas (si se envían)
            WITH s0, sLast, $stops AS stops
            UNWIND stops AS stopName
            MERGE (s:Station { name: stopName })
            WITH s0, sLast, collect(s) AS stopNodes

            // 4. Construir lista completa
            WITH [s0] + stopNodes + [sLast] AS stations

            // 5. Crear relaciones ROUTE
            UNWIND range(0, size(stations)-2) AS i
            WITH stations, stations[i] AS a, stations[i+1] AS b
            MERGE (a)-[r:ROUTE { Id: $id }]->(b)
            SET r.StartTime = $start,
                r.EndTime = $end,
                r.Status = $status
            WITH stations, collect(r) AS rels
            RETURN stations, rels[0] AS rel
        ";

        // Actualizar propiedades sin reconstruir la ruta
        public const string UpdateRouteProperties =
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

        // Verificar si ya está inactiva
        public const string CheckRouteInactive =
            @"
            MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
            WHERE ALL(r IN rels WHERE r.Id = $id)
            WITH [r IN rels WHERE r.Status = 'Inactive'] AS inactiveRels
            RETURN size(inactiveRels) AS inactiveCount
            LIMIT 1
        ";

        // Soft delete (cambiar status a Inactive)
        public const string SoftDeleteRoute =
            @"
            MATCH path=(origin:Station)-[rels:ROUTE*]->(dest:Station)
            WHERE ALL(r IN rels WHERE r.Id = $id)
            WITH rels, [n IN nodes(path) | n.name] AS stations, length(path) AS len
            ORDER BY len DESC
            LIMIT 1
            FOREACH (r IN rels | SET r.Status = 'Inactive')
            RETURN stations, rels[0] AS rel
        ";
    }
}
