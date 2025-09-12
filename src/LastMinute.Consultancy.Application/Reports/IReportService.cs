using System.Threading.Tasks;
using LastMinute.Consultancy.Application.Reports.Queries;

namespace LastMinute.Consultancy.Application.Reports
{
    public interface IReportService
    {
        Task<UtilizationReportDto> GetUtilizationAsync(GetUtilizationReportQuery query);
    }
}

