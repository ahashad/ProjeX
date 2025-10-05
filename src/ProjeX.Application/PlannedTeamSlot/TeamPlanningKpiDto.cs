namespace ProjeX.Application.PlannedTeamSlot
{
    public class TeamPlanningKpiDto
    {
        // Cost KPIs
        public decimal TotalPlannedCost { get; set; }
        public decimal TotalActualCost { get; set; }
        public decimal TotalVariance { get; set; }
        public decimal VariancePercentOfPlanned { get; set; }

        // Allocation KPIs (FTE as percentage)
        public decimal PlannedAllocationPercent { get; set; }
        public decimal UtilizedAllocationPercent { get; set; }
        public decimal RemainingAllocationPercent { get; set; }

        // Assignment Count KPIs
        public int ActiveAssignmentsCount { get; set; }
        public int CompletedAssignmentsCount { get; set; }

        // Utilization KPI
        public decimal AverageUtilizationPercent { get; set; }

        // Metadata for scope tracking
        public int PlannedSlotsCount { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}
