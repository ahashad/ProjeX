namespace ProjeX.Application.ActualAssignment
{
    public class AssignmentPreCheckResult
    {
        public List<string> Warnings { get; set; } = new();
        public List<string> Blockers { get; set; } = new();
        public bool RequiresApproval { get; set; }
        public bool HasCostVariance { get; set; }
        public bool HasUtilizationWarning { get; set; }
        public bool HasRoleMismatch { get; set; }
        public decimal CostVarianceAmount { get; set; }
        public decimal CostVariancePercentage { get; set; }
        
        public bool HasBlockers => Blockers.Any();
        public bool HasWarnings => Warnings.Any();
        public bool IsValid => !HasBlockers;
    }
}

