using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Application.Overhead
{
    public class OverheadDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public OverheadCategory Category { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        
        // Audit fields
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
    }
}


