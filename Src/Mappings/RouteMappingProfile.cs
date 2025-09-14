using AutoMapper;
using PerlaMetro_RouteService.Src.DTOs;
using PerlaMetro_RouteService.Src.Models;

namespace PerlaMetro_RouteService.Src.Mappings
{
    public class RouteMappingProfile : Profile
    {
        public RouteMappingProfile()
        {
            CreateMap<RouteDto, Models.Route>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => RouteStatus.Active));
            CreateMap<Models.Route, RouteDto>();
        }
    }
}
