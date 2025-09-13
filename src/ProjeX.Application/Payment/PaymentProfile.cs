using AutoMapper;
using ProjeX.Domain.Entities;

namespace ProjeX.Application.Payment
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<Domain.Entities.Payment, PaymentDto>()
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice.InvoiceNumber))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.Method.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ReferenceNumber, opt => opt.MapFrom(src => src.Reference));
        }
    }
}

