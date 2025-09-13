using ProjeX.Domain.Common;

namespace ProjeX.Domain.Entities
{
    public enum EmployeeStatus
    {
        Active,
        Inactive
    }

    public class Employee : AuditableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
        public RolesCatalog Role { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public decimal MonthlyIncentive { get; set; }
        public decimal CommissionPercent { get; set; }
        public bool IsActive { get; set; } = true;
        public EmployeeStatus Status => IsActive ? EmployeeStatus.Active : EmployeeStatus.Inactive;
    }
}


