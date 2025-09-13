using AutoMapper;
using ProjeX.Application.Budget.Commands;
using ProjeX.Domain.Entities;

namespace ProjeX.Application.Budget
{
    public class BudgetProfile : Profile
    {
        public BudgetProfile()
        {
            CreateMap<Domain.Entities.Budget, BudgetDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.PathName, opt => opt.MapFrom(src => src.Path != null ? src.Path.Name : string.Empty))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.ToString()))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedBy != null ? $"{src.ApprovedBy.FirstName} {src.ApprovedBy.LastName}" : string.Empty));

            CreateMap<CreateBudgetCommand, Domain.Entities.Budget>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.ActualAmount, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.CommittedAmount, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => false));

            CreateMap<UpdateBudgetCommand, Domain.Entities.Budget>()
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsApproved, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedById, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore());
        }
    }
}

