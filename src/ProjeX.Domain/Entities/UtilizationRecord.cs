using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class UtilizationRecord : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CapacityProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? ProjectId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public TimeBucket TimeBucket { get; set; }
        
        // Utilization data
        public decimal PlannedUtilizationPercentage { get; set; }
        public decimal ActualUtilizationPercentage { get; set; }
        public decimal PlannedHours { get; set; }
        public decimal ActualHours { get; set; }
        public decimal AvailableHours { get; set; }
        
        // Variance analysis
        public decimal UtilizationVariancePercentage => ActualUtilizationPercentage - PlannedUtilizationPercentage;
        public decimal HoursVariance => ActualHours - PlannedHours;
        
        // Flags and alerts
        public bool IsOverUtilized => ActualUtilizationPercentage > 100;
        public bool IsUnderUtilized => ActualUtilizationPercentage < 50;
        public bool RequiresRebalancing { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual CapacityProfile CapacityProfile { get; set; } = null!;
        public virtual Employee? Employee { get; set; }
        public virtual Project? Project { get; set; }
    }
}

