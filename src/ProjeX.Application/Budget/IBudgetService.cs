using ProjeX.Domain.Enums;

namespace ProjeX.Application.Budget
{
    public interface IBudgetService
    {
        Task<IEnumerable<BudgetDto>> GetAllAsync();
        Task<IEnumerable<BudgetDto>> GetByProjectIdAsync(Guid projectId);
        Task<IEnumerable<BudgetDto>> GetByPathIdAsync(Guid pathId);
        Task<BudgetDto?> GetByIdAsync(Guid id);
        Task<BudgetDto> CreateAsync(CreateBudgetRequest request, string userId);
        Task<BudgetDto> UpdateAsync(UpdateBudgetRequest request, string userId);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ApproveBudgetAsync(Guid id, string approvedById);
        Task<decimal> GetTotalBudgetByProjectAsync(Guid projectId);
        Task<decimal> GetTotalBudgetByCategoryAsync(Guid projectId, BudgetCategory category);
        Task<IEnumerable<BudgetDto>> GetBudgetsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> ValidateBudgetAgainstContractAsync(Guid projectId, decimal additionalAmount);
    }
}

