
namespace ProjeX.Application.RolesCatalog.Commands
{
    public class UpdateRolesCatalogCommand
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int Level { get; set; }
        public decimal DefaultSalary { get; set; }
        public decimal DefaultMonthlyIncentive { get; set; }
        public decimal CommissionPercent { get; set; }
        public string? Notes { get; set; }
    }
}


