using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerlaMetro_RouteService.Src.DTOs;
using PerlaMetro_RouteService.Src.Helpers;
using PerlaMetro_RouteService.Src.Interfaces;
using PerlaMetro_RouteService.Src.Models;

namespace PerlaMetro_RouteService.Src.Controllers
{
    /// <summary>
    /// Controlador para gestionar las rutas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RoutesController> _logger;

        /// <summary>
        /// Constructor para la clase RoutesController.
        /// </summary>
        /// <param name="routeRepository">Repositorio de rutas.</param>
        /// <param name="mapper">Mapper para convertir entre modelos y DTOs.</param>
        /// <param name="logger">Logger para registrar información y errores.</param>
        public RoutesController(
            IRouteRepository routeRepository,
            IMapper mapper,
            ILogger<RoutesController> logger
        )
        {
            _routeRepository = routeRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva ruta.
        /// </summary>
        /// <param name="routeDto">Datos de la ruta a crear.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateRoute([FromBody] CreateRouteDto routeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Error de validación al agregar ruta: {@Errors}",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    );
                    return BadRequest(
                        new ApiResponse<RouteDto>(
                            false,
                            "Error en los datos de entrada.",
                            null,
                            ModelState
                                .Values.SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList()
                        )
                    );
                }
                var route = _mapper.Map<Models.Route>(routeDto);
                var createdRoute = await _routeRepository.CreateRouteAsync(route);
                var response = new ApiResponse<Models.Route>(
                    true,
                    "Ruta creada exitosamente",
                    createdRoute
                );
                return CreatedAtAction(
                    nameof(GetRouteByGuid),
                    new { guid = createdRoute.Id },
                    response
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando la ruta: {Message}", ex.Message);
                var errorResponse = new ApiResponse<Models.Route>(
                    false,
                    "Error creando la ruta",
                    null,
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                );
                return BadRequest(errorResponse);
            }
        }

        /// <summary>
        /// Obtiene todas las rutas.
        /// </summary>
        /// <returns>Lista de rutas.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllRoutes()
        {
            try
            {
                var routes = await _routeRepository.GetAllRoutesAsync();
                var routeDtos = routes.Select(_mapper.Map<RouteDto>);
                return Ok(
                    new ApiResponse<IEnumerable<RouteDto>>(
                        true,
                        "Rutas recuperadas exitosamente",
                        routeDtos
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todas las rutas: {Message}", ex.Message);
                return NotFound(
                    new ApiResponse<IEnumerable<RouteDto>>(
                        false,
                        "Error obteniendo las rutas",
                        null,
                        new List<string> { "Ocurrió un error inesperado al recuperar las rutas." }
                    )
                );
            }
        }

        /// <summary>
        /// Obtiene una ruta por su GUID.
        /// </summary>
        /// <param name="guid">GUID de la ruta.</param>
        /// <returns>Datos de la ruta.</returns>
        [HttpGet("{guid}")]
        public async Task<IActionResult> GetRouteByGuid(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    _logger.LogWarning("GetRouteByGuid llamado con GUID nulo o vacío");
                    return BadRequest(
                        new ApiResponse<RouteDto>(
                            false,
                            "ID de ruta inválido",
                            null,
                            new List<string> { "El ID de la ruta no puede estar vacío." }
                        )
                    );
                }

                var route = await _routeRepository.GetRouteByGuidAsync(guid);
                if (route == null)
                {
                    return NotFound(
                        new ApiResponse<RouteDto>(
                            false,
                            "Ruta no encontrada",
                            null,
                            new List<string> { $"Ruta con ID {guid} no encontrada." }
                        )
                    );
                }

                if (route.Status == "Inactive")
                {
                    var inactiveDto = _mapper.Map<InactiveRouteDto>(route);
                    return Ok(
                        new ApiResponse<InactiveRouteDto>(
                            true,
                            "Ruta inactiva recuperada exitosamente",
                            inactiveDto
                        )
                    );
                }

                var dto = _mapper.Map<RouteDto>(route);
                return Ok(new ApiResponse<RouteDto>(true, "Ruta recuperada exitosamente", dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recuperando ruta {Guid}: {Message}", guid, ex.Message);
                return BadRequest(
                    new ApiResponse<RouteDto>(
                        false,
                        "Error obteniendo la ruta",
                        null,
                        new List<string> { "Ocurrió un error inesperado al recuperar la ruta." }
                    )
                );
            }
        }

        /// <summary>
        /// Actualiza una ruta existente.
        /// </summary>
        /// <param name="guid">GUID de la ruta a actualizar.</param>
        /// <param name="routeDto">Datos de la ruta a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPut("{guid}")]
        public async Task<IActionResult> UpdateRoute(
            string guid,
            [FromBody] UpdateRouteDto routeDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Error de validación al actualizar la ruta: {@Errors}",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    );
                    return BadRequest(
                        new ApiResponse<RouteDto>(
                            false,
                            "Datos de entrada no válidos",
                            null,
                            ModelState
                                .Values.SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList()
                        )
                    );
                }

                var existingRoute = await _routeRepository.GetRouteByGuidAsync(guid);
                if (existingRoute == null)
                {
                    return NotFound(
                        new ApiResponse<RouteDto>(
                            false,
                            "Ruta no encontrada",
                            null,
                            new List<string> { $"Ruta con ID {guid} no encontrada." }
                        )
                    );
                }

                bool originProvided = routeDto.Origin != null;
                bool destinationProvided = routeDto.Destination != null;
                bool stopsProvided = routeDto.Stops != null;
                bool startProvided = routeDto.StartTime.HasValue;
                bool endProvided = routeDto.EndTime.HasValue;

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
                    Stops = stopsProvided ? routeDto.Stops : existingRoute.Stops,
                };

                var result = await _routeRepository.UpdateRouteAsync(
                    finalRoute,
                    originProvided,
                    destinationProvided,
                    stopsProvided
                );
                if (result == null)
                {
                    return BadRequest(
                        new ApiResponse<RouteDto>(
                            false,
                            "Error al actualizar la ruta",
                            null,
                            ModelState
                                .Values.SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList()
                        )
                    );
                }

                var resultDto = _mapper.Map<RouteDto>(result);
                return Ok(new ApiResponse<RouteDto>(true, "Ruta actualizada con éxito", resultDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al actualizar la ruta {Guid}: {Message}",
                    guid,
                    ex.Message
                );
                return BadRequest(
                    new ApiResponse<RouteDto>(
                        false,
                        "Error al actualizar la ruta",
                        null,
                        new List<string> { "Ocurrió un error inesperado al actualizar la ruta." }
                    )
                );
            }
        }

        /// <summary>
        /// Elimina una ruta por su GUID.
        /// </summary>
        /// <param name="guid">GUID de la ruta a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpDelete("{guid}")]
        public async Task<IActionResult> DeleteRoute(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    _logger.LogWarning("DeleteRoute llamado con GUID nulo o vacío");
                    return BadRequest(
                        new ApiResponse<object>(
                            false,
                            "ID de ruta no válido",
                            null,
                            new List<string> { "El ID de la ruta no puede estar vacío." }
                        )
                    );
                }

                var existingRoute = await _routeRepository.GetRouteByGuidAsync(guid);
                if (existingRoute == null)
                {
                    return NotFound(
                        new ApiResponse<object>(
                            false,
                            "Ruta no encontrada",
                            null,
                            new List<string> { $"Ruta con ID {guid} no fue encontrada." }
                        )
                    );
                }

                await _routeRepository.DeleteRouteAsync(guid);
                return Ok(new ApiResponse<object>(true, "Ruta eliminada con éxito", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al eliminar la ruta {Guid}: {Message}",
                    guid,
                    ex.Message
                );
                return NotFound(
                    new ApiResponse<object>(
                        false,
                        "Error al eliminar la ruta",
                        null,
                        new List<string> { "Ocurrió un error inesperado al eliminar la ruta." }
                    )
                );
            }
        }
    }
}
