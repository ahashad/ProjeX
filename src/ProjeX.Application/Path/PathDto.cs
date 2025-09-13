namespace ProjeX.Application.Path
{
    public class PathDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public decimal AllowedAllocationPercentage { get; set; }
        public decimal PlannedCost { get; set; }
        public Guid? OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? ModifiedBy { get; set; }
        
        // Related data
        public int DeliverablesCount { get; set; }
        public int PlannedTeamSlotsCount { get; set; }
        public decimal TotalBudgetAmount { get; set; }
    }
}

