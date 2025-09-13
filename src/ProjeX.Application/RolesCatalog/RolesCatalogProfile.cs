using AutoMapper;
using ProjeX.Domain.Entities;

namespace ProjeX.Application.RolesCatalog
{
    public class RolesCatalogProfile : Profile
    {
        public RolesCatalogProfile()
        {
            CreateMap<ProjeX.Domain.Entities.RolesCatalog, RolesCatalogDto>();
        }
    }
}


