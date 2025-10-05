using System.ComponentModel.DataAnnotations;

namespace ProjeX.Application.ActualAssignment.Commands
{
    public class UpdateActualAssignmentCommand
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Allocation percent must be between 0 and 100")]
        public decimal AllocationPercent { get; set; }

        public string Notes { get; set; } = string.Empty;

        // Salary snapshot fields - optional overrides
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be non-negative")]
        public decimal? SnapshotSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Monthly incentive must be non-negative")]
        public decimal? SnapshotMonthlyIncentive { get; set; }

        [Range(0, 100, ErrorMessage = "Commission percent must be between 0 and 100")]
        public decimal? SnapshotCommissionPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tickets must be non-negative")]
        public decimal? SnapshotTickets { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Hoteling must be non-negative")]
        public decimal? SnapshotHoteling { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Others must be non-negative")]
        public decimal? SnapshotOthers { get; set; }
    }
}