using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjeX.Application.Reports;
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

        [HttpGet("project-summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetProjectSummaryReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _reportService.GenerateProjectSummaryReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating project summary report");
                return Problem("An error occurred while generating the project summary report.", 
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("time-tracking")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetTimeTrackingReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] Guid? employeeId)
        {
            try
            {
                var report = await _reportService.GenerateTimeTrackingReportAsync(startDate, endDate, employeeId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating time tracking report");
                return Problem("An error occurred while generating the time tracking report.", 
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("client-billing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetClientBillingReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] Guid? clientId)
        {
            try
            {
                var report = await _reportService.GenerateClientBillingReportAsync(startDate, endDate, clientId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating client billing report");
                return Problem("An error occurred while generating the client billing report.", 
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("employee-utilization")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetEmployeeUtilizationReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _reportService.GenerateEmployeeUtilizationReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating employee utilization report");
                return Problem("An error occurred while generating the employee utilization report.", 
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("export/project-summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ExportProjectSummaryReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string format = "pdf")
        {
            try
            {
                var fileData = await _reportService.ExportProjectSummaryReportAsync(startDate, endDate, format);
                
                var contentType = format.ToLower() switch
                {
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "csv" => "text/csv",
                    _ => "application/pdf"
                };

                var fileName = $"project-summary-{DateTime.Now:yyyy-MM-dd}.{format}";
                
                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting project summary report");
                return Problem("An error occurred while exporting the project summary report.", 
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("export/time-tracking")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ExportTimeTrackingReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] Guid? employeeId, [FromQuery] string format = "pdf")
        {
            try
            {
                var fileData = await _reportService.ExportTimeTrackingReportAsync(startDate, endDate, employeeId, format);
                
                var contentType = format.ToLower() switch
                {
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "csv" => "text/csv",
                    _ => "application/pdf"
                };

                var fileName = $"time-tracking-{DateTime.Now:yyyy-MM-dd}.{format}";
                
                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting time tracking report");
                return Problem("An error occurred while exporting the time tracking report.", 
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}

