using AutoMapper;
using LastMinute.Consultancy.Domain.Entities;

namespace LastMinute.Consultancy.Application.Client
{
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<LastMinute.Consultancy.Domain.Entities.Client, ClientDto>();
        }
    }
}


