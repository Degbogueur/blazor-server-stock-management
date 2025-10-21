using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Models;
using StockManagement.ViewModels.Operations;

namespace StockManagement.Services;

public interface IReportService
{
    Task<(IEnumerable<OperationViewModel> items, int totalCount)> GetOperationsAsync(
        int page,
        int pageSize,
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

internal class ReportService(StockDbContext dbContext) : IReportService
{
    public async Task<(IEnumerable<OperationViewModel> items, int totalCount)> GetOperationsAsync(
        int page,
        int pageSize,
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
        // Build separate queries for StockIn and StockOut
        IQueryable<StockInOperation> stockInQuery = dbContext.Set<StockInOperation>()
            .Include(o => o.Product)
            .Include(o => o.Supplier)
            .Where(o => !o.IsDeleted);

        IQueryable<StockOutOperation> stockOutQuery = dbContext.Set<StockOutOperation>()
            .Include(o => o.Product)
            .Include(o => o.Employee)
            .Where(o => !o.IsDeleted);

        // Apply operation type filter
        if (operationType.HasValue)
        {
            if (operationType.Value == OperationType.StockIn)
            {
                stockOutQuery = stockOutQuery.Where(o => false); // Exclude all StockOut
            }
            else
            {
                stockInQuery = stockInQuery.Where(o => false); // Exclude all StockIn
            }
        }

        // Apply date filters
        if (startDate.HasValue)
        {
            stockInQuery = stockInQuery.Where(o => o.Date >= startDate.Value);
            stockOutQuery = stockOutQuery.Where(o => o.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            stockInQuery = stockInQuery.Where(o => o.Date <= endDate.Value);
            stockOutQuery = stockOutQuery.Where(o => o.Date <= endDate.Value);
        }

        // Apply product filter
        if (productId.HasValue)
        {
            stockInQuery = stockInQuery.Where(o => o.ProductId == productId.Value);
            stockOutQuery = stockOutQuery.Where(o => o.ProductId == productId.Value);
        }

        // Apply supplier filter (only for StockIn)
        if (supplierId.HasValue)
        {
            stockInQuery = stockInQuery.Where(o => o.SupplierId == supplierId.Value);
            stockOutQuery = stockOutQuery.Where(o => false); // Exclude all StockOut
        }

        // Apply employee filter (only for StockOut)
        if (employeeId.HasValue)
        {
            stockInQuery = stockInQuery.Where(o => false); // Exclude all StockIn
            stockOutQuery = stockOutQuery.Where(o => o.EmployeeId == employeeId.Value);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            stockInQuery = stockInQuery.Where(o =>
                EF.Functions.ILike(o.Product!.Name, $"%{searchString}%") ||
                EF.Functions.ILike(o.Quantity.ToString(), $"%{searchString}%") ||
                EF.Functions.ILike(o.Supplier!.Name, $"%{searchString}%") ||
                EF.Functions.ILike("StockIn", $"%{searchString}%"));

            stockOutQuery = stockOutQuery.Where(o =>
                EF.Functions.ILike(o.Product!.Name, $"%{searchString}%") ||
                EF.Functions.ILike(o.Quantity.ToString(), $"%{searchString}%") ||
                EF.Functions.ILike(o.Employee!.FirstName, $"%{searchString}%") ||
                EF.Functions.ILike(o.Employee!.LastName, $"%{searchString}%") ||
                EF.Functions.ILike("StockOut", $"%{searchString}%"));
        }

        // Get counts from both queries
        var stockInCount = await stockInQuery.CountAsync();
        var stockOutCount = await stockOutQuery.CountAsync();
        var totalCount = stockInCount + stockOutCount;

        // Fetch data from both tables separately
        var stockInOps = await stockInQuery
            .Select(o => new OperationViewModel
            {
                OperationId = o.Id,
                ProductId = o.ProductId,
                ProductName = o.Product!.Name,
                Quantity = o.Quantity,
                Date = o.Date,
                Type = OperationType.StockIn,
                SupplierId = o.SupplierId,
                SupplierName = o.Supplier!.Name,
                EmployeeId = null,
                EmployeeFullName = null,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        var stockOutOps = await stockOutQuery
            .Select(o => new OperationViewModel
            {
                OperationId = o.Id,
                ProductId = o.ProductId,
                ProductName = o.Product!.Name,
                Quantity = o.Quantity,
                Date = o.Date,
                Type = OperationType.StockOut,
                SupplierId = null,
                SupplierName = null,
                EmployeeId = o.EmployeeId,
                EmployeeFullName = o.Employee!.FirstName + " " + o.Employee.LastName,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        // Combine and sort in memory (only the filtered subset)
        var combined = stockInOps.Concat(stockOutOps);

        // Apply sorting
        combined = sortBy switch
        {
            nameof(OperationViewModel.ProductName) => sortDescending
                ? combined.OrderByDescending(o => o.ProductName)
                : combined.OrderBy(o => o.ProductName),
            nameof(OperationViewModel.Quantity) => sortDescending
                ? combined.OrderByDescending(o => o.Quantity)
                : combined.OrderBy(o => o.Quantity),
            nameof(OperationViewModel.Date) => sortDescending
                ? combined.OrderByDescending(o => o.Date)
                : combined.OrderBy(o => o.Date),
            nameof(OperationViewModel.Type) => sortDescending
                ? combined.OrderByDescending(o => o.Type)
                : combined.OrderBy(o => o.Type),
            nameof(OperationViewModel.SupplierName) => sortDescending
                ? combined.OrderByDescending(o => o.SupplierName)
                : combined.OrderBy(o => o.SupplierName),
            nameof(OperationViewModel.EmployeeFullName) => sortDescending
                ? combined.OrderByDescending(o => o.EmployeeFullName)
                : combined.OrderBy(o => o.EmployeeFullName),
            _ => combined.OrderByDescending(o => o.Date)
                         .ThenByDescending(o => o.CreatedAt) // Default sort
        };

        // Apply pagination
        var pagedData = combined
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedData, totalCount);
    }
}
