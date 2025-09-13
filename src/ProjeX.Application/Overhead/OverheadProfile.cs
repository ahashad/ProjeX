using AutoMapper;
using ProjeX.Domain.Entities;

namespace ProjeX.Application.Overhead
{
    public class OverheadProfile : Profile
    {
        public OverheadProfile()
        {
            CreateMap<ProjeX.Domain.Entities.Overhead, OverheadDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName));
        }
    }
}

