using System.ComponentModel.DataAnnotations;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Application.TimeEntry.Commands
{
    public class UpdateTimeEntryCommand
    {
        [Required]
        public Guid Id { get; set; }
   
        [Required]
        public Guid ActualAssignmentId { get; set; }
    
 [Required]
        [DataType(DataType.Date)]
   [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Today;
   
     [Required]
        [Range(0.25, 24.0, ErrorMessage = "Hours must be between 0.25 and 24.0")]
      [Display(Name = "Hours")]
        public decimal Hours { get; set; }
        
   [Required]
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    [Display(Name = "Description")]
public string Description { get; set; } = string.Empty;
        
 [Display(Name = "Status")]
        public TimeEntryStatus Status { get; set; }
        
    [Display(Name = "Is Billable")]
        public bool IsBillable { get; set; } = true;
        
        [Range(0, double.MaxValue, ErrorMessage = "Billable rate must be greater than or equal to 0")]
        [Display(Name = "Billable Rate")]
        public decimal? BillableRate { get; set; }
    }
}