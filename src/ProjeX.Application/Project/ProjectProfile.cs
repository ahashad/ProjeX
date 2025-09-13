using AutoMapper;
using ProjeX.Domain.Entities;

namespace ProjeX.Application.Project
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            CreateMap<ProjeX.Domain.Entities.Project, ProjectDto>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.ClientName));
        }
    }
}


