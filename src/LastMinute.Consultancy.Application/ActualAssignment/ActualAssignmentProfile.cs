using AutoMapper;
using LastMinute.Consultancy.Domain.Entities;

namespace LastMinute.Consultancy.Application.ActualAssignment
{
    public class ActualAssignmentProfile : Profile
    {
        public ActualAssignmentProfile()
        {
            CreateMap<Domain.Entities.ActualAssignment, ActualAssignmentDto>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Project.Client.ClientName))
                .ForMember(dest => dest.ProjectStartDate, opt => opt.MapFrom(src => src.Project.StartDate))
                .ForMember(dest => dest.ProjectEndDate, opt => opt.MapFrom(src => src.Project.EndDate))
                .ForMember(dest => dest.PlannedTeamSlotDescription, opt => opt.MapFrom(src => $"{src.PlannedTeamSlot.Role.RoleName} - {src.PlannedTeamSlot.PeriodMonths} months"))
                .ForMember(dest => dest.PlannedPeriodMonths, opt => opt.MapFrom(src => src.PlannedTeamSlot.PeriodMonths))
                .ForMember(dest => dest.PlannedAllocationPercent, opt => opt.MapFrom(src => src.PlannedTeamSlot.AllocationPercent))
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.EmployeeEmail, opt => opt.MapFrom(src => src.Employee.Email))
                .ForMember(dest => dest.EmployeePhone, opt => opt.MapFrom(src => src.Employee.Phone))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.PlannedTeamSlot.RoleId))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.PlannedTeamSlot.Role.RoleName))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));
        }
    }
}

