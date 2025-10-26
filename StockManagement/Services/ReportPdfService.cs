using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StockManagement.Models;
using StockManagement.ViewModels.Operations;
using StockManagement.ViewModels.Requests;

namespace StockManagement.Services;

public interface IReportPdfService
{
    Task<byte[]> GenerateOperationsReportAsync(
        string? searchString = null,
        string? sortBy = null,
        bool sortDescending = false,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? productId = null,
        int? supplierId = null,
        int? employeeId = null,
        OperationType? operationType = null);
}

internal class ReportPdfService : IReportPdfService
{
    private readonly IReportService _reportService;

    public ReportPdfService(IReportService reportService)
    {
        _reportService = reportService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateOperationsReportAsync(
        string? searchString = null,
        string? sortBy = null,
        bool sortDescending = false,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? productId = null,
        int? supplierId = null,
        int? employeeId = null,
        OperationType? operationType = null)
    {
        var request = new DataGridRequest
        {
            Page = 0,
            PageSize = int.MaxValue,
            SearchTerm = searchString,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var filters = new OperationFiltersViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            ProductId = productId,
            SupplierId = supplierId,
            EmployeeId = employeeId,
            Type = operationType
        };

        var result = await _reportService.GetStockOperationsAsync(request, filters);

        var operations = result.Items.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header().Element(ComposeHeader);

                // Content
                page.Content().Element(content => ComposeContent(content, operations,
                    startDate, endDate, searchString, operationType));

                // Footer
                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Stock Operations Report")
                    .FontSize(20)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private static void ComposeContent(IContainer container, List<OperationViewModel> operations,
        DateTime? startDate, DateTime? endDate, string? searchString, OperationType? operationType)
    {
        container.Column(column =>
        {
            // Filter information
            column.Item().PaddingBottom(10).Row(row =>
            {
                if (startDate.HasValue || endDate.HasValue || !string.IsNullOrEmpty(searchString) || operationType.HasValue)
                {
                    row.RelativeItem().Column(filterColumn =>
                    {
                        filterColumn.Item().Text("Applied Filters:").Bold().FontSize(11);

                        if (startDate.HasValue)
                            filterColumn.Item().Text($"Start Date: {startDate.Value:yyyy-MM-dd}").FontSize(9);

                        if (endDate.HasValue)
                            filterColumn.Item().Text($"End Date: {endDate.Value:yyyy-MM-dd}").FontSize(9);

                        if (!string.IsNullOrEmpty(searchString))
                            filterColumn.Item().Text($"Search: {searchString}").FontSize(9);

                        if (operationType.HasValue)
                            filterColumn.Item().Text($"Type: {operationType.Value}").FontSize(9);
                    });
                }

                row.RelativeItem().AlignRight().Column(summaryColumn =>
                {
                    summaryColumn.Item().Text($"Total Operations: {operations.Count}").Bold().FontSize(11);
                    summaryColumn.Item().Text($"Total Quantity: {operations.Sum(o => o.Quantity):N0}").FontSize(9);
                });
            });

            // Table
            column.Item().Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Product
                    columns.RelativeColumn(1); // Quantity
                    columns.RelativeColumn(1.5f); // Date
                    columns.RelativeColumn(1.5f); // Type
                    columns.RelativeColumn(2); // Supplier
                    columns.RelativeColumn(2); // Employee
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Product").Bold();
                    header.Cell().Element(CellStyle).Text("Quantity").Bold();
                    header.Cell().Element(CellStyle).Text("Date").Bold();
                    header.Cell().Element(CellStyle).Text("Type").Bold();
                    header.Cell().Element(CellStyle).Text("Supplier").Bold();
                    header.Cell().Element(CellStyle).Text("Employee").Bold();

                    static IContainer CellStyle(IContainer container)
                    {
                        return container
                            .Background(Colors.Grey.Lighten2)
                            .Padding(5)
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Darken1);
                    }
                });

                // Rows
                foreach (var operation in operations)
                {
                    var isStockIn = operation.Type == OperationType.StockIn;
                    var backgroundColor = isStockIn ? Colors.Green.Lighten4 : Colors.Red.Lighten4;

                    table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                        .Text(operation.ProductName);

                    table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                        .AlignRight()
                        .Text(operation.Quantity.ToString("N0"));

                    table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                        .Text(operation.Date.ToShortDateString());

                    table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                        .Text(operation.Type.ToString());

                    table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                        .Text(operation.SupplierName ?? "-");

                    table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                        .Text(operation.EmployeeFullName ?? "-");
                }

                static IContainer RowCellStyle(IContainer container, string backgroundColor)
                {
                    return container
                        .Background(backgroundColor)
                        .Padding(5)
                        .BorderBottom(0.5f)
                        .BorderColor(Colors.Grey.Lighten1);
                }
            });

            // Summary section
            if (operations.Count != 0)
            {
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(summaryColumn =>
                    {
                        summaryColumn.Item().Text("Summary by Type:").Bold().FontSize(11);

                        var stockInOps = operations.Where(o => o.Type == OperationType.StockIn).ToList();
                        var stockOutOps = operations.Where(o => o.Type == OperationType.StockOut).ToList();

                        if (stockInOps.Count != 0)
                        {
                            summaryColumn.Item().Text($"Stock In: {stockInOps.Count} operations, Total Qty: {stockInOps.Sum(o => o.Quantity):N0}")
                                .FontSize(9)
                                .FontColor(Colors.Green.Darken2);
                        }

                        if (stockOutOps.Count != 0)
                        {
                            summaryColumn.Item().Text($"Stock Out: {stockOutOps.Count} operations, Total Qty: {stockOutOps.Sum(o => o.Quantity):N0}")
                                .FontSize(9)
                                .FontColor(Colors.Red.Darken2);
                        }
                    });
                });
            }
        });
    }
}
