using System.Threading.Tasks;
using LastMinute.Consultancy.Application.Invoice.Commands;

namespace LastMinute.Consultancy.Application.Invoice
{
    public interface IInvoiceService
    {
        Task<InvoiceDto> PlanAsync(PlanInvoiceCommand command);
        Task<InvoiceDto> ConfirmAsync(ConfirmInvoiceCommand command);
        Task<InvoiceDto> CancelAsync(CancelInvoiceCommand command);
        Task<List<InvoiceDto>> GetAllAsync(Guid? projectId = null, Guid? clientId = null);
        Task<InvoiceDto?> GetByIdAsync(Guid id);
        Task<InvoiceDto> UpdateAsync(Guid id, PlanInvoiceCommand command, string userId);
        Task<InvoiceDto> MarkAsSentAsync(Guid id, string userId);
        Task<InvoiceDto> MarkAsPaidAsync(Guid id, decimal amount, string paymentReference, string userId);
        Task<List<InvoiceDto>> GetOverdueInvoicesAsync();
        Task<decimal> GetTotalReceivablesAsync();
        Task<InvoiceDto> GenerateFromTimeEntriesAsync(Guid projectId, DateTime fromDate, DateTime toDate, string userId);
    }
}

