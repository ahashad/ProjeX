using System.Threading.Tasks;
using ProjeX.Application.Reports.Queries;

namespace ProjeX.Application.Reports
{
    public interface IReportService
    {
        Task<UtilizationReportDto> GetUtilizationAsync(GetUtilizationReportQuery query);
    }
}

