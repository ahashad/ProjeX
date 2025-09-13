using System.ComponentModel.DataAnnotations;

namespace ProjeX.Application.ActualAssignment.Commands
{
    public class UnassignActualAssignmentCommand
    {
        [Required]
        public Guid AssignmentId { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}
