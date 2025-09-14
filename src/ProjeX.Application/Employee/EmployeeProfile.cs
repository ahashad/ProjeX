using AutoMapper;
using ProjeX.Domain.Entities;

namespace ProjeX.Application.Employee
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<ProjeX.Domain.Entities.Employee, EmployeeDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()));
        }
    }
}


