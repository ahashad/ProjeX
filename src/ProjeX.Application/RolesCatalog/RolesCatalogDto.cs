using ProjeX.Domain.Entities;

namespace ProjeX.Application.RolesCatalog
{
    public class RolesCatalogDto
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int Level { get; set; }
        public decimal DefaultSalary { get; set; }
        public decimal DefaultMonthlyIncentive { get; set; }
        public decimal CommissionPercent { get; set; }
        public string? Notes { get; set; }
        
        // Audit fields
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
    }
}


