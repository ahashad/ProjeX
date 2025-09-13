using ProjeX.Domain.Enums;

namespace ProjeX.Application.Overhead.Commands
{
    public class UpdateOverheadCommand
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public OverheadCategory Category { get; set; }
        public Guid ProjectId { get; set; }
    }
}


