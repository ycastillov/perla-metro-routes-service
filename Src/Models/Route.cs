using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerlaMetro_RouteService.Src.Models
{
    public class Route
    {
        public required string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Origin { get; set; }
        public required string Destination { get; set; }
        public required TimeSpan StartTime { get; set; }
        public required TimeSpan EndTime { get; set; }
        public required List<string>? Stops { get; set; }
        public required string Status { get; set; }
    }
}
