using System;
using System.Threading.Tasks;
using LastMinute.Consultancy.Application.Payment.Commands;

namespace LastMinute.Consultancy.Application.Payment
{
    public interface IPaymentService
    {
        Task<PaymentDto> RecordAsync(RecordPaymentCommand command);
    }
}

