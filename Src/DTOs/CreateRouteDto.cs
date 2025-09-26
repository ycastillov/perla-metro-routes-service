using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerlaMetro_RouteService.Src.DTOs
{
    public class CreateRouteDto
    {
        public required string Origin { get; set; }
        public required string Destination { get; set; }
        public required TimeSpan StartTime { get; set; }
        public required TimeSpan EndTime { get; set; }
        public List<string>? Stops { get; set; }
        public string Status { get; set; } = "Active";
    }
}
