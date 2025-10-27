using Microsoft.AspNetCore.Mvc;
using StockManagement.Extensions;
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

        var fileName = $"Operations_Report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("download-inventory-report/{inventoryId}")]
    public async Task<IActionResult> DownloadInventoryReport(int inventoryId, [FromQuery] string inventoryCode)
    {
        try
        {
            var userName = User.GetUserFullName() ?? User.Identity?.Name ?? "Unknown User";

            var pdfBytes = await pdfService.GenerateInventoryReportAsync(inventoryId, userName);

            var fileName = $"{inventoryCode}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating PDF: {ex.Message}");
        }
    }

    [HttpGet("preview-inventory-report/{inventoryId}")]
    public async Task<IActionResult> PreviewInventoryReport(int inventoryId)
    {
        try
        {
            var userName = User.GetUserFullName() ?? User.Identity?.Name ?? "Unknown User";

            var pdfBytes = await pdfService.GenerateInventoryReportAsync(inventoryId, userName);

            return File(pdfBytes, "application/pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating PDF: {ex.Message}");
        }
    }

    [HttpGet("download-stock-levels-report")]
    public async Task<IActionResult> DownloadStockLevelsReport([FromQuery] DateTime? asOfDate = null)
    {
        try
        {
            var userName = User.GetUserFullName() ?? User.Identity?.Name ?? "Unknown User";

            var pdfBytes = await pdfService.GenerateStockLevelsReportAsync(asOfDate, userName);

            var fileName = $"Stock_Levels" +
                $"{(asOfDate != null ? $"_As_For_{asOfDate.Value.ToShortDateString()}" : "")}" +
                $"_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating PDF: {ex.Message}");
        }
    }

    [HttpGet("preview-stock-levels-report")]
    public async Task<IActionResult> PreviewStockLevelsReport([FromQuery] DateTime? asOfDate = null)
    {
        try
        {
            var userName = User.GetUserFullName() ?? User.Identity?.Name ?? "Unknown User";

            var pdfBytes = await pdfService.GenerateStockLevelsReportAsync(asOfDate, userName);

            return File(pdfBytes, "application/pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating PDF: {ex.Message}");
        }
    }
}
