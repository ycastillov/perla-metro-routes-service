using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerlaMetro_RouteService.Src.DTOs;
using PerlaMetro_RouteService.Src.Interfaces;
using PerlaMetro_RouteService.Src.Models;

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
            var route = _mapper.Map<Models.Route>(routeDto);
            var createdRoute = await _routeRepository.CreateRouteAsync(route);
            return CreatedAtAction(
                nameof(CreateRoute),
                new { guid = createdRoute.Id },
                createdRoute
            );
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> GetRouteByGuid(string guid)
        {
            var route = await _routeRepository.GetRouteByGuidAsync(guid);
            if (route == null)
                return NotFound();

            if (route.Status == RouteStatus.Inactive.ToString())
            {
                InactiveRouteDto inactiveDto = _mapper.Map<InactiveRouteDto>(route);
                return Ok(inactiveDto);
            }

            RouteDto routeDto = _mapper.Map<RouteDto>(route);
            return Ok(routeDto);
        }

        [HttpGet]
        // [Authorize]
        public async Task<IActionResult> GetAllRoutes()
        {
            var routes = await _routeRepository.GetAllRoutesAsync();
            return Ok(routes);
        }

        [HttpPut("{guid}")]
        public async Task<IActionResult> UpdateRoute(string guid, [FromBody] RouteDto routeDto)
        {
            var existingRoute = await _routeRepository.GetRouteByGuidAsync(guid);
            if (existingRoute == null)
                return NotFound();

            var updatedRoute = _mapper.Map(routeDto, existingRoute);
            updatedRoute.Id = guid; // Ensure the ID remains unchanged
            var result = await _routeRepository.UpdateRouteAsync(updatedRoute);

            if (result == null)
                return StatusCode(500, "An error occurred while updating the route.");

            RouteDto resultDto = _mapper.Map<RouteDto>(result);
            return Ok(resultDto);
        }

        [HttpDelete("{guid}")]
        // [Authorize]
        public async Task<IActionResult> DeleteRoute(string guid)
        {
            var existingRoute = await _routeRepository.GetRouteByGuidAsync(guid);
            if (existingRoute == null)
                return NotFound();

            await _routeRepository.DeleteRouteAsync(guid);
            return NoContent();
        }
    }
}
