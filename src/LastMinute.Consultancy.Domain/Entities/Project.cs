using LastMinute.Consultancy.Domain.Common;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Domain.Entities
{
    public class Project : AuditableEntity
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public ProjectStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // New required fields for PlannedTeam ? ActualAssignment flow
        public decimal ExpectedWorkingPeriodMonths { get; set; }
        public decimal ProjectPrice { get; set; }
    }
}


