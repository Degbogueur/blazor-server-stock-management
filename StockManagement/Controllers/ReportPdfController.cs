using Microsoft.AspNetCore.Mvc;
using StockManagement.Models;
using StockManagement.Services;

namespace StockManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportPdfController(IReportPdfService pdfService) : Controller
{
    [HttpGet("download-operations-report")]
    public async Task<IActionResult> DownloadOperationsReport(
        [FromQuery] string? searchString = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? productId = null,
        [FromQuery] int? supplierId = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] OperationType? operationType = null)
    {
        var pdfBytes = await pdfService.GenerateOperationsReportAsync(
            searchString: searchString,
            sortBy: sortBy,
            sortDescending: sortDescending,
            startDate: startDate,
            endDate: endDate,
            productId: productId,
            supplierId: supplierId,
            employeeId: employeeId,
            operationType: operationType
        );

        var fileName = $"Operations_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }
}
