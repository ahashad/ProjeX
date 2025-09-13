using AutoMapper;
using ProjeX.Domain.Entities;

namespace ProjeX.Application.Client
{
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<ProjeX.Domain.Entities.Client, ClientDto>();
        }
    }
}


