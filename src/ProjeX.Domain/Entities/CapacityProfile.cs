using ProjeX.Domain.Common;

namespace ProjeX.Domain.Entities
{
    public class CapacityProfile : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? VendorRoleId { get; set; }
        public decimal HoursPerWeek { get; set; }
        public decimal FtePercentage { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        // Holiday and PTO considerations
        public decimal AnnualLeaveHours { get; set; }
        public decimal PublicHolidayHours { get; set; }
        public decimal TrainingHours { get; set; }
        
        // Navigation properties
        public virtual Employee? Employee { get; set; }
        public virtual RolesCatalog? VendorRole { get; set; }
        public virtual ICollection<UtilizationRecord> UtilizationRecords { get; set; } = new List<UtilizationRecord>();
    }
}

