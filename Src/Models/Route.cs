using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerlaMetro_RouteService.Src.Models
{
    public class Route
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<string>? Stops { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
