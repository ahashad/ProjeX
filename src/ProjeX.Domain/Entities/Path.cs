using ProjeX.Domain.Common;

namespace ProjeX.Domain.Entities
{
    public class Path : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public decimal AllowedAllocationPercentage { get; set; } // 0-100
        public decimal PlannedCost { get; set; }
        public Guid? OwnerId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual Employee? Owner { get; set; }
        public virtual ICollection<Deliverable> Deliverables { get; set; } = new List<Deliverable>();
        public virtual ICollection<PlannedTeamSlot> PlannedTeamSlots { get; set; } = new List<PlannedTeamSlot>();
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    }
}

