using ProjeX.Domain.Enums;

namespace ProjeX.Application.VendorManagement
{
    public interface IVendorService
    {
        Task<VendorDto> CreateAsync(CreateVendorRequest request);
        Task<VendorDto> UpdateAsync(UpdateVendorRequest request);
        Task<VendorDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<VendorDto>> GetAllAsync(VendorStatus? status = null, VendorCategory? category = null);
        Task<ThreeWayMatchResult> PerformThreeWayMatchAsync(Guid vendorInvoiceId);
        Task<VendorPerformanceDto> GetVendorPerformanceAsync(Guid vendorId, DateTime fromDate, DateTime toDate);
        Task<bool> DeleteAsync(Guid id);
    }
}