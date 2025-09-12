using AutoMapper;
using LastMinute.Consultancy.Domain.Entities;

namespace LastMinute.Consultancy.Application.Project
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            CreateMap<LastMinute.Consultancy.Domain.Entities.Project, ProjectDto>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.ClientName));
        }
    }
}


