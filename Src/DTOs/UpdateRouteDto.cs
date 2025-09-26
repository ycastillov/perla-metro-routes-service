using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerlaMetro_RouteService.Src.DTOs
{
    public class UpdateRouteDto
    {
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public List<string>? Stops { get; set; }
        public string? Status { get; set; }
    }
}
