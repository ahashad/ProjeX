using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class Overhead : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public OverheadCategory Category { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
    }
}


