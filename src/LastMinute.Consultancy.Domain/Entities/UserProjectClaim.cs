using LastMinute.Consultancy.Domain.Common;

namespace LastMinute.Consultancy.Domain.Entities
{
    public class UserProjectClaim : AuditableEntity
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public string ClaimType { get; set; } = string.Empty;
        public string ClaimValue { get; set; } = string.Empty;
    }
}