using System;
using System.Threading.Tasks;
using ProjeX.Application.Payment.Commands;

namespace ProjeX.Application.Payment
{
    public interface IPaymentService
    {
        Task<PaymentDto> RecordAsync(RecordPaymentCommand command);
    }
}

