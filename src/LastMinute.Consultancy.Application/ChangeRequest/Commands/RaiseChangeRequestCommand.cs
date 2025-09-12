using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Application.ChangeRequest.Commands
{
    public class RaiseChangeRequestCommand
    {
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ChangeRequestType ChangeRequestType { get; set; } = ChangeRequestType.ScopeChange;
        public ChangeRequestPriority Priority { get; set; } = ChangeRequestPriority.Medium;
        public decimal EstimatedCost { get; set; }
        public int EstimatedHours { get; set; }
        public string Justification { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
    }
}

