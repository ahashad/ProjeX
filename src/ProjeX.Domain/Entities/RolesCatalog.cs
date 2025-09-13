using ProjeX.Domain.Common;

namespace ProjeX.Domain.Entities
{
    public class RolesCatalog : AuditableEntity
    {
        public string RoleName { get; set; }
        public int Level { get; set; }
        public decimal DefaultSalary { get; set; }
        public decimal DefaultMonthlyIncentive { get; set; }
        public decimal CommissionPercent { get; set; }
        public string Notes { get; set; }
    }
}


