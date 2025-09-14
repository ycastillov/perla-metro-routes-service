using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerlaMetro_RouteService.Src.DTOs
{
    public class CreateRouteDto
    {
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; } = TimeOnly.MinValue;
        public TimeOnly EndTime { get; set; } = TimeOnly.MinValue;
        public List<string> Stops { get; set; } = new();
    }
}
