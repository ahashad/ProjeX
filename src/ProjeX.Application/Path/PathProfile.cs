using AutoMapper;
using ProjeX.Domain.Entities;

namespace ProjeX.Application.Path
{
    public class PathProfile : Profile
    {
        public PathProfile()
        {
            CreateMap<Domain.Entities.Path, PathDto>()
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner != null ? $"{src.Owner.FirstName} {src.Owner.LastName}" : string.Empty))
                .ForMember(dest => dest.DeliverablesCount, opt => opt.MapFrom(src => src.Deliverables.Count))
                .ForMember(dest => dest.PlannedTeamSlotsCount, opt => opt.MapFrom(src => src.PlannedTeamSlots.Count))
                .ForMember(dest => dest.TotalBudgetAmount, opt => opt.MapFrom(src => src.Budgets.Sum(b => b.PlannedAmount)));

            CreateMap<CreatePathRequest, Domain.Entities.Path>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdatePathRequest, Domain.Entities.Path>()
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
        }
    }
}

