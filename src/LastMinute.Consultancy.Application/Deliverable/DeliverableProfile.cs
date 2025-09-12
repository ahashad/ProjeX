using AutoMapper;
using LastMinute.Consultancy.Domain.Entities;

namespace LastMinute.Consultancy.Application.Deliverable
{
    public class DeliverableProfile : Profile
    {
        public DeliverableProfile()
        {
            CreateMap<LastMinute.Consultancy.Domain.Entities.Deliverable, DeliverableDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName));
        }
    }
}

