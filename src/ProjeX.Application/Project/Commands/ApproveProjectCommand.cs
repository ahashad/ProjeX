using ProjeX.Application.Project;
using System.ComponentModel.DataAnnotations;

namespace ProjeX.Application.Project.Commands
{
    public class ApproveProjectCommand
    {
        [Required]
        public Guid ProjectId { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public string ApprovalNotes { get; set; } = string.Empty;
        
        public DateTime ApprovedDate { get; set; } = DateTime.UtcNow;
    }
}

