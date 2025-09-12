using AutoMapper;
using LastMinute.Consultancy.Domain.Entities;

namespace LastMinute.Consultancy.Application.Invoice
{
    public class InvoiceProfile : Profile
    {
        public InvoiceProfile()
        {
            CreateMap<Domain.Entities.Invoice, InvoiceDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.ClientName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<InvoiceLineItem, InvoiceLineItemDto>()
                .ForMember(dest => dest.LineItemType, opt => opt.MapFrom(src => src.Type.ToString()));
        }
    }
}

