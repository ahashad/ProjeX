using LastMinute.Consultancy.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace LastMinute.Consultancy.Application.Project.Commands
{
    public class UpdateProjectCommand
    {
        public Guid Id { get; set; }
        
        [Required]
        public string ProjectName { get; set; } = string.Empty;
        
        [Required]
        public Guid ClientId { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Budget must be positive")]
        public decimal Budget { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Project price must be positive")]
        public decimal ProjectPrice { get; set; }
        
        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Expected working period must be greater than 0")]
        public decimal ExpectedWorkingPeriodMonths { get; set; }
        
        public ProjectStatus Status { get; set; }
        
        public string Notes { get; set; } = string.Empty;
    }
}


