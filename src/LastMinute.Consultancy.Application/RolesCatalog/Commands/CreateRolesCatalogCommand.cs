using LastMinute.Consultancy.Application.RolesCatalog;

namespace LastMinute.Consultancy.Application.RolesCatalog.Commands
{
    public class CreateRolesCatalogCommand
    {
        public string RoleName { get; set; } = string.Empty;
        public int Level { get; set; }
        public decimal DefaultSalary { get; set; }
        public decimal DefaultMonthlyIncentive { get; set; }
        public decimal CommissionPercent { get; set; }
        public string? Notes { get; set; }
    }
}


