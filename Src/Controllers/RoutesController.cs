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
    public class RoutesController : ControllerBase
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IMapper _mapper;

        public RoutesController(IRouteRepository routeRepository, IMapper mapper)
        {
            _routeRepository = routeRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoute([FromBody] CreateRouteDto routeDto)
        {
            var route = _mapper.Map<Models.Route>(routeDto);
            var createdRoute = await _routeRepository.CreateRouteAsync(route);
            return CreatedAtAction(
                nameof(GetRouteByGuid),
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

            if (route.Status == "Inactive")
            {
                var inactiveDto = _mapper.Map<InactiveRouteDto>(route);
                return Ok(inactiveDto);
            }

            var dto = _mapper.Map<RouteDto>(route);
            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoutes()
        {
            var routes = await _routeRepository.GetAllRoutesAsync();
            return Ok(routes.Select(_mapper.Map<RouteDto>));
        }

        [HttpPut("{guid}")]
        public async Task<IActionResult> UpdateRoute(
            string guid,
            [FromBody] UpdateRouteDto routeDto
        )
        {
            var existingRoute = await _routeRepository.GetRouteByGuidAsync(guid);
            if (existingRoute == null)
                return NotFound();

            // 1) Detectar intención del cliente
            bool originProvided = routeDto.Origin != null;
            bool destinationProvided = routeDto.Destination != null;
            bool stopsProvided = routeDto.Stops != null;
            bool startProvided = routeDto.StartTime.HasValue;
            bool endProvided = routeDto.EndTime.HasValue;

            // (Opcional) log para debug
            // _logger.LogDebug("UpdateRoute flags: originProvided={originProvided}, stopsProvided={stopsProvided}", originProvided, stopsProvided);

            // 2) Fusionar valores (finalRoute) — solo reemplazar si el cliente lo envió
            var finalRoute = new Models.Route
            {
                Id = guid,
                Origin = originProvided ? routeDto.Origin! : existingRoute.Origin,
                Destination = destinationProvided
                    ? routeDto.Destination!
                    : existingRoute.Destination,
                StartTime = startProvided ? routeDto.StartTime!.Value : existingRoute.StartTime,
                EndTime = endProvided ? routeDto.EndTime!.Value : existingRoute.EndTime,
                Status = existingRoute.Status,
                Stops = stopsProvided ? routeDto.Stops : existingRoute.Stops, // aquí stopsProvided diferencia null vs []
            };

            // 3) llamar repositorio con flags para que decida la query correcta
            var result = await _routeRepository.UpdateRouteAsync(
                finalRoute,
                originProvided,
                destinationProvided,
                stopsProvided
            );

            if (result == null)
                return StatusCode(500, "An error occurred while updating the route.");

            var resultDto = _mapper.Map<RouteDto>(result);
            return Ok(resultDto);
        }

        [HttpDelete("{guid}")]
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
