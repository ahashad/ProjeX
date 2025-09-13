using ProjeX.Domain.Enums;

namespace ProjeX.Application.ChangeRequest.Commands
{
    public class ProcessChangeRequestCommand
    {
        public Guid ChangeRequestId { get; set; }
        public ChangeRequestStatus NewStatus { get; set; }
        public string ApprovalNotes { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
    }
}

