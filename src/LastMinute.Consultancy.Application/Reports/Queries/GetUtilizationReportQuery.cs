namespace LastMinute.Consultancy.Application.Reports.Queries
{
    public class GetUtilizationReportQuery
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? EmployeeId { get; set; } // Optional filter for specific employee
    }
}

