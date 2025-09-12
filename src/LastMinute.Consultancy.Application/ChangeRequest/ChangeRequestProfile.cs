using AutoMapper;
using LastMinute.Consultancy.Domain.Entities;

namespace LastMinute.Consultancy.Application.ChangeRequest
{
    public class ChangeRequestProfile : Profile
    {
        public ChangeRequestProfile()
        {
            CreateMap<Domain.Entities.ChangeRequest, ChangeRequestDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.ChangeRequestType, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Justification, opt => opt.MapFrom(src => src.BusinessJustification))
                .ForMember(dest => dest.ApprovalNotes, opt => opt.MapFrom(src => src.ApprovalComments));
        }
    }
}

