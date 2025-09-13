using ProjeX.Domain.Enums;

namespace ProjeX.Application.CRM
{
 public interface ICrmService
  {
      Task<CrmDashboardDto> GetDashboardAsync();
        Task<AccountDto> CreateAccountAsync(CreateAccountRequest request);
        Task<AccountDto?> GetAccountByIdAsync(Guid id);
    Task<IEnumerable<AccountDto>> GetAccountsAsync(AccountStatus? status = null, string? industry = null);
        Task<OpportunityDto> CreateOpportunityAsync(CreateOpportunityRequest request);
  Task<OpportunityDto?> GetOpportunityByIdAsync(Guid id);
      Task<TenderDto?> GetTenderByIdAsync(Guid id);
      Task<IEnumerable<TenderDto>> GetTendersAsync(TenderStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<TenderDto> CreateTenderAsync(CreateTenderRequest request);
    }

    public class CreateTenderRequest
  {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
     public Guid ClientId { get; set; }
   public decimal EstimatedValue { get; set; }
        public DateTime SubmissionDeadline { get; set; }
        public decimal BondAmount { get; set; }
     public string Requirements { get; set; } = string.Empty;
 public string Notes { get; set; } = string.Empty;
    }
}