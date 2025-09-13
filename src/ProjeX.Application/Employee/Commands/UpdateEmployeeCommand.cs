using ProjeX.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProjeX.Application.Employee.Commands
{
    public class UpdateEmployeeCommand
    {
        public Guid Id { get; set; }
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        public Guid RoleId { get; set; }
        
        [Required]
        public DateTime HireDate { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal Salary { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal MonthlyIncentive { get; set; }
        
        [Range(0, 100)]
        public decimal CommissionPercent { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}


