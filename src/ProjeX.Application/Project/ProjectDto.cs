using ProjeX.Domain.Enums;

namespace ProjeX.Application.Project
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public decimal ProjectPrice { get; set; }
        public decimal ExpectedWorkingPeriodMonths { get; set; }
        public ProjectStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // Audit fields
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
    }
}


