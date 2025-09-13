using AutoMapper;

namespace ProjeX.Application.PlannedTeamSlot
{
    public class PlannedTeamSlotProfile : Profile
    {
        public PlannedTeamSlotProfile()
        {
            CreateMap<Domain.Entities.PlannedTeamSlot, PlannedTeamSlotDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.IsAssigned, opt => opt.Ignore()); // This will be set manually
        }
    }
}