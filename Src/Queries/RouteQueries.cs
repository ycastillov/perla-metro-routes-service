using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerlaMetro_RouteService.Src.Queries
{
    public static class RouteQueries
    {
        public const string CreateRoute =
            @"
            CREATE (r:Route {
                Id: $id,
                Origin: $origin,
                Destination: $destination,
                StartTime: $start,
                EndTime: $end,
                Stops: $stops,
                Status: $status
            })";

        public const string GetAllRoutes =
            @"
            MATCH (r:Route)
            RETURN r";

        public const string GetRouteByGuid =
            @"
            MATCH (r:Route { Id: $id })
            RETURN r";

        public const string SoftDeleteRoute =
            @"
            MATCH (r:Route { Id: $id })
            SET r.Status = 'Inactive'
            RETURN r";
    }
}
