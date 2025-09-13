using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class Project : AuditableEntity
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public decimal ContractValue { get; set; }
        public string Currency { get; set; } = "USD";
        public string PaymentTerms { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public Guid? ProjectManagerId { get; set; }
        public decimal PlannedMargin { get; set; }
        public decimal ActualMargin { get; set; }
        public bool IsApproved { get; set; }
        public Guid? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }
        
        // Existing fields
        public decimal ExpectedWorkingPeriodMonths { get; set; }
        public decimal ProjectPrice { get; set; }

        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        public virtual Employee? ProjectManager { get; set; }
        public virtual Employee? ApprovedBy { get; set; }
        public virtual ICollection<Deliverable> Deliverables { get; set; } = new List<Deliverable>();
        public virtual ICollection<Path> Paths { get; set; } = new List<Path>();
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
        public virtual ICollection<PlannedTeamSlot> PlannedTeamSlots { get; set; } = new List<PlannedTeamSlot>();
        public virtual ICollection<ActualAssignment> ActualAssignments { get; set; } = new List<ActualAssignment>();
        public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
        public virtual ICollection<Overhead> Overheads { get; set; } = new List<Overhead>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}


