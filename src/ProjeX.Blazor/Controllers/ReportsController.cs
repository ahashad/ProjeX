using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjeX.Application.Reports;
using ProjeX.Application.Reports.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ProjeX.Blazor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet("utilization")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UtilizationReportDto>> GetUtilizationReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var query = new GetUtilizationReportQuery
                {
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MaxValue
                };
                
                var report = await _reportService.GetUtilizationAsync(query);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating utilization report");
                return Problem("An error occurred while generating the utilization report.", 
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        // Placeholder methods for future implementation
        [HttpGet("project-summary")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public ActionResult GetProjectSummaryReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Project summary report is not yet implemented");
        }

        [HttpGet("time-tracking")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public ActionResult GetTimeTrackingReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] Guid? employeeId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Time tracking report is not yet implemented");
        }

        [HttpGet("client-billing")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public ActionResult GetClientBillingReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] Guid? clientId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Client billing report is not yet implemented");
        }

        [HttpGet("employee-utilization")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public ActionResult GetEmployeeUtilizationReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Employee utilization report is not yet implemented");
        }

        [HttpGet("export/project-summary")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public ActionResult ExportProjectSummaryReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string format = "pdf")
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Project summary export is not yet implemented");
        }

        [HttpGet("export/time-tracking")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public ActionResult ExportTimeTrackingReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] Guid? employeeId, [FromQuery] string format = "pdf")
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Time tracking export is not yet implemented");
        }
    }
}

