using AutoMapper;
using ProjeX.Domain.Entities;

namespace ProjeX.Application.ActualAssignment
{
    public class ActualAssignmentProfile : Profile
    {
        public ActualAssignmentProfile()
        {
            CreateMap<Domain.Entities.ActualAssignment, ActualAssignmentDto>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : string.Empty))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Project != null && src.Project.Client != null ? src.Project.Client.ClientName : string.Empty))
                .ForMember(dest => dest.ProjectStartDate, opt => opt.MapFrom(src => src.Project != null ? src.Project.StartDate : DateTime.MinValue))
                .ForMember(dest => dest.ProjectEndDate, opt => opt.MapFrom(src => src.Project != null ? src.Project.EndDate : DateTime.MinValue))
                .ForMember(dest => dest.PlannedTeamSlotDescription, opt => opt.MapFrom(src => 
                    src.PlannedTeamSlot != null && src.PlannedTeamSlot.Role != null 
                        ? $"{src.PlannedTeamSlot.Role.RoleName} - {src.PlannedTeamSlot.PeriodMonths} months" 
                        : string.Empty))
                .ForMember(dest => dest.PlannedPeriodMonths, opt => opt.MapFrom(src => src.PlannedTeamSlot != null ? src.PlannedTeamSlot.PeriodMonths : 0))
                .ForMember(dest => dest.PlannedAllocationPercent, opt => opt.MapFrom(src => src.PlannedTeamSlot != null ? src.PlannedTeamSlot.AllocationPercent : 0))
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
                .ForMember(dest => dest.EmployeeEmail, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Email : string.Empty))
                .ForMember(dest => dest.EmployeePhone, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Phone : string.Empty))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.PlannedTeamSlot != null ? src.PlannedTeamSlot.RoleId : Guid.Empty))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => 
                    src.PlannedTeamSlot != null && src.PlannedTeamSlot.Role != null 
                        ? src.PlannedTeamSlot.Role.RoleName 
                        : string.Empty))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));
        }
    }
}

