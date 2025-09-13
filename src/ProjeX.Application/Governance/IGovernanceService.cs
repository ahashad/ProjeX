namespace ProjeX.Application.Governance
{
    public interface IGovernanceService
    {
    Task<ApprovalRequestDto?> GetApprovalRequestByIdAsync(Guid id);
 Task<BudgetEncumbranceDto?> GetBudgetEncumbranceByIdAsync(Guid id);
        Task<ChangeOrderDto?> GetChangeOrderByIdAsync(Guid id);
    }
}