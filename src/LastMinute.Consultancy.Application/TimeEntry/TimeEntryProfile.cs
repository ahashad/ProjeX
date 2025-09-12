using AutoMapper;
using LastMinute.Consultancy.Domain.Entities;

namespace LastMinute.Consultancy.Application.TimeEntry
{
    public class TimeEntryProfile : Profile
    {
    public TimeEntryProfile()
        {
  CreateMap<Domain.Entities.TimeEntry, TimeEntryDto>()
.ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ActualAssignment.Project.ProjectName))
        .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.ActualAssignment.Project.Client.ClientName))
 .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.ActualAssignment.Employee.FullName))
.ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.ActualAssignment.PlannedTeamSlot.Role.RoleName));
    }
 }
}