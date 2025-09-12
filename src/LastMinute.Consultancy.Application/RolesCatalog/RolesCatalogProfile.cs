using AutoMapper;
using LastMinute.Consultancy.Domain.Entities;

namespace LastMinute.Consultancy.Application.RolesCatalog
{
    public class RolesCatalogProfile : Profile
    {
        public RolesCatalogProfile()
        {
            CreateMap<LastMinute.Consultancy.Domain.Entities.RolesCatalog, RolesCatalogDto>();
        }
    }
}


