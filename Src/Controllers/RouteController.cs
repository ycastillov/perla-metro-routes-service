using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PerlaMetro_RouteService.Src.DTOs;
using PerlaMetro_RouteService.Src.Interfaces;

namespace PerlaMetro_RouteService.Src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RouteController(IRouteRepository routeRepository, IMapper mapper) : ControllerBase
    {
        private readonly IRouteRepository _routeRepository = routeRepository;
        private readonly IMapper _mapper = mapper;

        [HttpPost]
        public async Task<IActionResult> CreateRoute([FromBody] RouteDto routeDto)
        {
            Models.Route route = _mapper.Map<Models.Route>(routeDto);
            await _routeRepository.CreateRouteAsync(route);
            return CreatedAtAction(nameof(GetRouteByGuid), new { guid = route.Id }, route);
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> GetRouteByGuid(string guid)
        {
            var route = await _routeRepository.GetRouteByGuidAsync(guid);
            if (route == null)
                return NotFound();

            RouteDto routeDto = _mapper.Map<RouteDto>(route);
            return Ok(routeDto);
        }
    }
}
