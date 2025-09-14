using ProjeX.Domain.Enums;

namespace ProjeX.Application.Project
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal Budget { get; set; }
        public decimal ContractValue { get; set; }
        public string Currency { get; set; } = "USD";
        public string PaymentTerms { get; set; } = string.Empty;
        public decimal ProjectPrice { get; set; }
        public decimal ExpectedWorkingPeriodMonths { get; set; }
        public decimal PlannedMargin { get; set; }
        public decimal ActualMargin { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedById { get; set; }
        public Guid? ProjectManagerId { get; set; }
        public Guid? AccountId { get; set; }
        public ProjectStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // Audit fields
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
    }
}


