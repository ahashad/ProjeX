using AutoMapper;
using ProjeX.Domain.Entities;

namespace ProjeX.Application.Deliverable
{
    public class DeliverableProfile : Profile
    {
        public DeliverableProfile()
        {
            CreateMap<ProjeX.Domain.Entities.Deliverable, DeliverableDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName));
        }
    }
}

