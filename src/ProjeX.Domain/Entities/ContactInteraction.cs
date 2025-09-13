using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
  public class ContactInteraction : AuditableEntity
    {
        public Guid Id { get; set; }
  public Guid ContactId { get; set; }
 public InteractionType InteractionType { get; set; }
        public string Subject { get; set; } = string.Empty;
      public string Description { get; set; } = string.Empty;
       public DateTime InteractionDate { get; set; }
    public string Method { get; set; } = string.Empty; // Email, Phone, Meeting, etc.
     public Guid? InitiatedById { get; set; }
 public string Outcome { get; set; } = string.Empty;
        public DateTime? FollowUpDate { get; set; }
 public bool IsFollowUpRequired { get; set; }
      public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual Contact Contact { get; set; } = null!;
        public virtual Employee? InitiatedBy { get; set; }
    }
}