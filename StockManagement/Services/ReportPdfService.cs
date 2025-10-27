using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StockManagement.Models;
using StockManagement.ViewModels.Inventories;
using StockManagement.ViewModels.Operations;
using StockManagement.ViewModels.Products;
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

    Task<byte[]> GenerateInventoryReportAsync(int inventoryId, string userName);

    Task<byte[]> GenerateStockLevelsReportAsync(DateTime? asOfDate, string userName);
}

internal class ReportPdfService : IReportPdfService
{
    private readonly IReportService _reportService;
    private readonly IInventoryService _inventoryService;

    public ReportPdfService(
        IReportService reportService,
        IInventoryService inventoryService)
    {
        _reportService = reportService;
        _inventoryService = inventoryService;
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
                page.Header().Element(ComposeOperationsReportHeader);

                // Content
                page.Content().Element(content => ComposeOperationsReportContent(content, operations,
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

    private static void ComposeOperationsReportHeader(IContainer container)
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

    private static void ComposeOperationsReportContent(IContainer container, List<OperationViewModel> operations,
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

    public async Task<byte[]> GenerateInventoryReportAsync(int inventoryId, string userName)
    {
        // Get inventory data
        var inventory = await _inventoryService.GetInventoryDetailsAsync(inventoryId);
        if (inventory == null)
            throw new InvalidOperationException($"Inventory with ID {inventoryId} not found.");

        var inventoryRows = await _inventoryService.GetRowsViewModelAsync(inventoryId);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header().Element(header => ComposeInventoryReportHeader(header, inventory, userName));

                // Content
                page.Content().Element(content => ComposeInventoryReportContent(content, inventoryRows));

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

    private static void ComposeInventoryReportHeader(IContainer container, InventoryViewModel inventory, string userName)
    {
        container.Column(column =>
        {
            // Title
            column.Item().AlignCenter().Text("INVENTORY REPORT")
                .FontSize(20)
                .Bold()
                .FontColor(Colors.Blue.Darken2);

            column.Item().PaddingVertical(5);

            // User info and timestamp
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Generated by: {userName}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);

                row.RelativeItem().AlignRight().Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            });

            column.Item().PaddingVertical(5);

            // Inventory details box
            column.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(detailsColumn =>
            {
                detailsColumn.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Inventory Code").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text(inventory.Code).FontSize(11).Bold();
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Date").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text(inventory.Date.ToShortDateString()).FontSize(11).Bold();
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Status").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text(inventory.Status.ToString()).FontSize(11).Bold()
                            .FontColor(inventory.Status == InventoryStatus.Completed ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                    });
                });

                detailsColumn.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Expected Units").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text(inventory.TotalExpectedUnits.ToString("N0")).FontSize(11).Bold();
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Counted Units").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text(inventory.TotalCountedUnits.ToString("N0")).FontSize(11).Bold();
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Variance").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text($"{(inventory.Variance >= 0 ? "+" : "")}{inventory.Variance}")
                            .FontSize(11).Bold()
                            .FontColor(inventory.Variance == 0 ? Colors.Green.Darken2 :
                                      inventory.Variance > 0 ? Colors.Blue.Darken2 : Colors.Red.Darken2);
                    });
                });
            });

            column.Item().PaddingVertical(10);
        });
    }

    private static void ComposeInventoryReportContent(IContainer container, List<InventoryRowViewModel> rows)
    {
        container.Table(table =>
        {
            // Define columns
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(60);  // Code
                columns.RelativeColumn(3);   // Product Name
                columns.ConstantColumn(70);  // Expected
                columns.ConstantColumn(70);  // Counted
                columns.ConstantColumn(60);  // Status
                columns.ConstantColumn(60);  // Variance
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("Code").Bold();
                header.Cell().Element(HeaderCellStyle).Text("Product Name").Bold();
                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Expected").Bold();
                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Counted").Bold();
                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Status").Bold();
                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Variance").Bold();

                static IContainer HeaderCellStyle(IContainer container)
                {
                    return container
                        .Background(Colors.Grey.Lighten2)
                        .Padding(5)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Darken1);
                }
            });

            // Rows
            foreach (var row in rows)
            {
                var variance = row.CountedQuantity - row.ExpectedQuantity;
                var isMatch = variance == 0;
                var backgroundColor = isMatch ? Colors.Green.Lighten4 : Colors.Orange.Lighten4;

                table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                    .Text(row.ProductCode ?? "-");

                table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                    .Text(row.ProductName);

                table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                    .AlignCenter()
                    .Text(row.ExpectedQuantity.ToString("N0"));

                table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                    .AlignCenter()
                    .Text(row.CountedQuantity.ToString("N0"));

                table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                    .AlignCenter()
                    .Text(isMatch ? "Match" : "Diff")
                    .FontColor(isMatch ? Colors.Green.Darken2 : Colors.Orange.Darken2)
                    .Bold();

                table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                    .AlignCenter()
                    .Text(variance == 0 ? "-" : $"{(variance > 0 ? "+" : "")}{variance}")
                    .FontColor(variance == 0 ? Colors.Black :
                              variance > 0 ? Colors.Blue.Darken2 : Colors.Red.Darken2);
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
    }

    public async Task<byte[]> GenerateStockLevelsReportAsync(DateTime? asOfDate, string userName)
    {
        // Get all products without pagination for PDF
        var request = new DataGridRequest
        {
            Page = 0,
            PageSize = int.MaxValue,
            SearchTerm = null,
            SortBy = nameof(ProductViewModel.Name),
            SortDescending = false
        };

        var result = await _reportService.GetStockLevelsAsync(request, asOfDate);
        var products = result.Items.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header().Element(header => ComposeStockLevelsReportHeader(header, asOfDate, userName, products.Count));

                // Content
                page.Content().Element(content => ComposeStockLevelsReportContent(content, products));

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

    private static void ComposeStockLevelsReportHeader(IContainer container, DateTime? asOfDate, string userName, int totalProducts)
    {
        container.Column(column =>
        {
            // Title
            column.Item().AlignCenter().Text("STOCK LEVELS REPORT")
                .FontSize(20)
                .Bold()
                .FontColor(Colors.Blue.Darken2);

            column.Item().PaddingVertical(5);

            // User info and timestamp
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Generated by: {userName}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);

                row.RelativeItem().AlignRight().Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            });

            column.Item().PaddingVertical(5);

            // Report details box
            column.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Row(detailsRow =>
            {
                detailsRow.RelativeItem().Column(col =>
                {
                    col.Item().Text("Report Date").FontSize(8).FontColor(Colors.Grey.Darken1);
                    col.Item().Text(asOfDate.HasValue ? asOfDate.Value.ToShortDateString() : DateTime.Today.ToShortDateString())
                        .FontSize(11).Bold();
                });

                detailsRow.RelativeItem().Column(col =>
                {
                    col.Item().Text("Report Type").FontSize(8).FontColor(Colors.Grey.Darken1);
                    col.Item().Text(asOfDate.HasValue ? "Historical Stock Levels" : "Current Stock Levels")
                        .FontSize(11).Bold();
                });

                detailsRow.RelativeItem().Column(col =>
                {
                    col.Item().Text("Total Products").FontSize(8).FontColor(Colors.Grey.Darken1);
                    col.Item().Text(totalProducts.ToString("N0"))
                        .FontSize(11).Bold();
                });
            });

            column.Item().PaddingVertical(10);
        });
    }

    private static void ComposeStockLevelsReportContent(IContainer container, List<ProductViewModel> products)
    {
        // Calculate statistics
        var totalStock = products.Sum(p => p.CurrentStock);
        var lowStockCount = products.Count(p => p.CurrentStock <= p.MinimumStockLevel && p.CurrentStock > 0);
        var outOfStockCount = products.Count(p => p.CurrentStock <= 0);

        container.Column(column =>
        {
            // Summary section
            column.Item().PaddingBottom(10).Row(summaryRow =>
            {
                summaryRow.RelativeItem().Background(Colors.Blue.Lighten4).Padding(8).Column(col =>
                {
                    col.Item().Text("Total Stock Units").FontSize(8).FontColor(Colors.Grey.Darken1);
                    col.Item().Text(totalStock.ToString("N0")).FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                });

                summaryRow.RelativeItem().Background(Colors.Orange.Lighten4).Padding(8).Column(col =>
                {
                    col.Item().Text("Low Stock Products").FontSize(8).FontColor(Colors.Grey.Darken1);
                    col.Item().Text(lowStockCount.ToString()).FontSize(14).Bold().FontColor(Colors.Orange.Darken2);
                });

                summaryRow.RelativeItem().Background(Colors.Red.Lighten4).Padding(8).Column(col =>
                {
                    col.Item().Text("Out of Stock").FontSize(8).FontColor(Colors.Grey.Darken1);
                    col.Item().Text(outOfStockCount.ToString()).FontSize(14).Bold().FontColor(Colors.Red.Darken2);
                });
            });

            // Products table
            column.Item().Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(70);   // Code
                    columns.RelativeColumn(4);    // Product Name
                    columns.ConstantColumn(80);   // Current Stock
                    columns.ConstantColumn(80);   // Min Level
                    columns.ConstantColumn(70);   // Status
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("Code").Bold();
                    header.Cell().Element(HeaderCellStyle).Text("Product Name").Bold();
                    header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Current Stock").Bold();
                    header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Min Level").Bold();
                    header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Status").Bold();

                    static IContainer HeaderCellStyle(IContainer container)
                    {
                        return container
                            .Background(Colors.Grey.Lighten2)
                            .Padding(5)
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Darken1);
                    }
                });

                // Rows
                foreach (var product in products)
                {
                    var isOutOfStock = product.CurrentStock <= 0;
                    var isLowStock = product.CurrentStock <= product.MinimumStockLevel && product.CurrentStock > 0;
                    var isHealthy = product.CurrentStock > product.MinimumStockLevel;

                    var backgroundColor = isOutOfStock ? Colors.Red.Lighten4 :
                                         isLowStock ? Colors.Orange.Lighten4 :
                                         Colors.Green.Lighten4;

                    var statusText = isOutOfStock ? "Out of Stock" :
                                    isLowStock ? "Low Stock" :
                                    "Healthy";

                    var statusColor = isOutOfStock ? Colors.Red.Darken2 :
                                     isLowStock ? Colors.Orange.Darken2 :
                                     Colors.Green.Darken2;

                    table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                        .Text(product.Code ?? "-");

                    table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                        .Text(product.Name);

                    table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                        .AlignCenter()
                        .Text(product.CurrentStock.ToString("N0"))
                        .FontColor(statusColor);

                    table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                        .AlignCenter()
                        .Text(product.MinimumStockLevel.ToString("N0"));

                    table.Cell().Element(container => RowCellStyle(container, backgroundColor))
                        .AlignCenter()
                        .Text(statusText)
                        .FontSize(8)
                        .FontColor(statusColor)
                        .Bold();
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

            // Legend
            column.Item().PaddingTop(15).Row(legendRow =>
            {
                legendRow.RelativeItem().Row(row =>
                {
                    row.ConstantItem(15).Height(15).Background(Colors.Green.Lighten4);
                    row.ConstantItem(5);
                    row.AutoItem().Text("Healthy Stock").FontSize(8);
                });

                legendRow.RelativeItem().Row(row =>
                {
                    row.ConstantItem(15).Height(15).Background(Colors.Orange.Lighten4);
                    row.ConstantItem(5);
                    row.AutoItem().Text("Low Stock (≤ Min Level)").FontSize(8);
                });

                legendRow.RelativeItem().Row(row =>
                {
                    row.ConstantItem(15).Height(15).Background(Colors.Red.Lighten4);
                    row.ConstantItem(5);
                    row.AutoItem().Text("Out of Stock").FontSize(8);
                });
            });
        });
    }
}
