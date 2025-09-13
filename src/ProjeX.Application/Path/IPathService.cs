namespace ProjeX.Application.Path
{
    public interface IPathService
    {
        Task<IEnumerable<PathDto>> GetAllAsync();
        Task<IEnumerable<PathDto>> GetByProjectIdAsync(Guid projectId);
        Task<PathDto?> GetByIdAsync(Guid id);
        Task<PathDto> CreateAsync(CreatePathRequest request, string userId);
        Task<PathDto> UpdateAsync(UpdatePathRequest request, string userId);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ValidatePathAllocationAsync(Guid projectId, decimal allocationPercentage, Guid? excludePathId = null);
        Task<decimal> GetTotalProjectAllocationAsync(Guid projectId);
        Task<IEnumerable<PathDto>> GetActivePathsByProjectAsync(Guid projectId);
    }
}

