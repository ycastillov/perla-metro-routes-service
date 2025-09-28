using AutoMapper;
using PerlaMetro_RouteService.Src.DTOs;
using PerlaMetro_RouteService.Src.Models;

namespace PerlaMetro_RouteService.Src.Mappings
{
    public class RouteMappingProfile : Profile
    {
        public RouteMappingProfile()
        {
            // Crear
            CreateMap<CreateRouteDto, Models.Route>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()));

            // Actualizar
            CreateMap<UpdateRouteDto, Models.Route>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Leer
            CreateMap<Models.Route, RouteDto>();
            CreateMap<Models.Route, InactiveRouteDto>();
        }
    }
}
