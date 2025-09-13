using ProjeX.Domain.Enums;

namespace ProjeX.Application.TimeEntry
{
    public class TimeEntryDto
    {
        public Guid Id { get; set; }
  public Guid ActualAssignmentId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
        public string Description { get; set; } = string.Empty;
        public TimeEntryStatus Status { get; set; }
        public bool IsBillable { get; set; } = true;
        public decimal? BillableRate { get; set; }
      public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
   public DateTime? ModifiedAt { get; set; }
      public string ModifiedBy { get; set; } = string.Empty;
    }
}