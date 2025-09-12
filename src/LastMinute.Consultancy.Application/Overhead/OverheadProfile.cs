using AutoMapper;
using LastMinute.Consultancy.Domain.Entities;

namespace LastMinute.Consultancy.Application.Overhead
{
    public class OverheadProfile : Profile
    {
        public OverheadProfile()
        {
            CreateMap<LastMinute.Consultancy.Domain.Entities.Overhead, OverheadDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName));
        }
    }
}

