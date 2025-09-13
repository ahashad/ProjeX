using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
  public class TenderActivity : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid TenderId { get; set; }
        public ActivityType ActivityType { get; set; }
    public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
public DateTime ActivityDate { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? AssignedToId { get; set; }
        public ActivityStatus Status { get; set; }
    public int Priority { get; set; } = 3;
    public string Outcome { get; set; } = string.Empty;
     public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual Tender Tender { get; set; } = null!;
     public virtual Employee? AssignedTo { get; set; }
    }
}