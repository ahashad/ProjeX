namespace ProjeX.Application.InvoicePlanning
{
    public interface IInvoicePlanService
    {
        Task<InvoicePlanDto> CreateAsync(CreateInvoicePlanRequest request);
        Task<InvoicePlanDto> UpdateAsync(UpdateInvoicePlanRequest request);
        Task<InvoicePlanDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<InvoicePlanDto>> GetByProjectIdAsync(Guid projectId);
        Task<IEnumerable<InvoiceScheduleDto>> GetUpcomingInvoicesAsync(DateTime fromDate, DateTime toDate);
        Task<InvoiceGenerationResult> GenerateInvoiceAsync(Guid scheduleId);
        Task<bool> DeleteAsync(Guid id);
    }
}

