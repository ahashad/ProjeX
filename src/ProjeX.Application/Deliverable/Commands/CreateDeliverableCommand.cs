using ProjeX.Domain.Enums;

namespace ProjeX.Application.Deliverable.Commands
{
    public class CreateDeliverableCommand
    {
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DeliverableStatus Status { get; set; }
    }
}

