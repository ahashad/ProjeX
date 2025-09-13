using ProjeX.Domain.Enums;

namespace ProjeX.Application.CRM
{
    public class CrmDashboardDto
    {
        public int TotalAccounts { get; set; }
        public int ActiveOpportunities { get; set; }
        public decimal TotalPipelineValue { get; set; }
        public decimal WeightedPipelineValue { get; set; }
        public int ProjectsWon { get; set; }
        public int ProjectsLost { get; set; }
        public decimal WinRate { get; set; }
        public List<OpportunityDto> RecentOpportunities { get; set; } = new();
        public List<AccountDto> TopAccounts { get; set; } = new();
        
 // Additional properties referenced in CrmService
        public int TotalOpportunities { get; set; }
      public int TotalTenders { get; set; }
        public decimal PipelineValue { get; set; }
        public int WonOpportunities { get; set; }
  public int ActiveTenders { get; set; }
     public List<ActivityDto> RecentActivities { get; set; } = new();
    }

    public class AccountDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public AccountStatus Status { get; set; }
    public string Industry { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
   public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public Guid? AccountManagerId { get; set; }
      public string AccountManagerName { get; set; } = string.Empty;
        public decimal AnnualRevenue { get; set; }
        public int EmployeeCount { get; set; }
        public bool IsActive { get; set; }
  public DateTime CreatedAt { get; set; }
        public List<ContactDto> Contacts { get; set; } = new();
      public List<OpportunityDto> Opportunities { get; set; } = new();
    }

    public class OpportunityDto
    {
        public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
      public string OpportunityNumber { get; set; } = string.Empty;
   public Guid AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
      public OpportunityStage Stage { get; set; }
  public string StageName { get; set; } = string.Empty;
        public decimal EstimatedValue { get; set; }
     public int ProbabilityPercent { get; set; }
        public DateTime ExpectedCloseDate { get; set; }
      public DateTime? ActualCloseDate { get; set; }
        public Guid? PrimaryContactId { get; set; }
        public string PrimaryContactName { get; set; } = string.Empty;
      public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; }
     public DateTime CreatedAt { get; set; }
    }

    public class ContactDto
    {
        public Guid Id { get; set; }
 public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
   public string JobTitle { get; set; } = string.Empty;
     public string Department { get; set; } = string.Empty;
        public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
      public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Additional DTOs referenced in services
    public class ActivityDto
    {
      public Guid Id { get; set; }
 public string Type { get; set; } = string.Empty;
  public string Subject { get; set; } = string.Empty;
     public string Description { get; set; } = string.Empty;
    public DateTime ActivityDate { get; set; }
     public string Contact { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
    }

    public class PipelineStageDto
 {
     public string StageName { get; set; } = string.Empty;
        public int OpportunityCount { get; set; }
    public decimal TotalValue { get; set; }
        public decimal WeightedValue { get; set; }
        
  // Additional properties used in CrmService
      public OpportunityStage Stage { get; set; }
   public int Count { get; set; }
  public decimal Value { get; set; }
    }

    public class SalesPipelineDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
       public string StageName { get; set; } = string.Empty;
        public decimal EstimatedValue { get; set; }
   public int ProbabilityPercent { get; set; }
   public DateTime ExpectedCloseDate { get; set; }
 public string AccountName { get; set; } = string.Empty;
  public string ContactName { get; set; } = string.Empty;
        
// Additional properties referenced in CrmService
    public int TotalOpportunities { get; set; }
    public decimal TotalValue { get; set; }
      public decimal WeightedValue { get; set; }
        public List<PipelineStageDto> StageBreakdown { get; set; } = new();
    }

    public class CreateAccountRequest
    {
      public string CompanyName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
     public string Website { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
      public Guid? AccountManagerId { get; set; }
     public decimal AnnualRevenue { get; set; }
  public int EmployeeCount { get; set; }
    }

    public class CreateOpportunityRequest
    {
        public string Name { get; set; } = string.Empty;
        public Guid AccountId { get; set; }
     public decimal EstimatedValue { get; set; }
     public int ProbabilityPercent { get; set; } = 50;
       public DateTime ExpectedCloseDate { get; set; }
        public Guid? PrimaryContactId { get; set; }
       public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class TenderDto
    {
     public Guid Id { get; set; }
        public string TenderNumber { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
      public string ClientName { get; set; } = string.Empty;
public decimal EstimatedValue { get; set; }
        public DateTime SubmissionDeadline { get; set; }
   public DateTime? SubmittedDate { get; set; }
   public string Status { get; set; } = string.Empty;
        public decimal BondAmount { get; set; }
        public string Requirements { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
      public bool IsActive { get; set; }
      public DateTime CreatedAt { get; set; }
      public string CreatedBy { get; set; } = string.Empty;
    }
}