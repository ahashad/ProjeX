using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Application.Overhead.Commands
{
    public class CreateOverheadCommand
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public OverheadCategory Category { get; set; }
        public Guid ProjectId { get; set; }
    }
}


