using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Models;
using StockManagement.ViewModels.Operations;
using StockManagement.ViewModels.Products;
using StockManagement.ViewModels.Requests;
using StockManagement.ViewModels.Results;

namespace StockManagement.Services;

public interface IReportService
{
    Task<(IEnumerable<OperationViewModel> items, int totalCount)> GetStockOperationsAsync(
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

    IQueryable<StockCardProductViewModel> GetStockCardProductsListQuery();

    Task<DataGridResult<ProductViewModel>> GetStockLevelsAsync(
        DataGridRequest request,
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default);
}

internal class ReportService(
    IDbContextFactory<StockDbContext> dbContextFactory,
    StockDbContext dbContext) : IReportService
{
    public async Task<(IEnumerable<OperationViewModel> items, int totalCount)> GetStockOperationsAsync(
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
        using var dbContext = dbContextFactory.CreateDbContext();

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
            .AsNoTracking()
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
            .AsNoTracking()
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

    public IQueryable<StockCardProductViewModel> GetStockCardProductsListQuery()
    {
        return dbContext.Products
            .OrderBy(p => p.Name)
            .Select(p => new StockCardProductViewModel
            {
                ProductId = p.Id,
                ProductName = p.Name,
                ProductCode = p.Code,
                TotalStockIn = p.Operations
                    .Where(o => o.Type == OperationType.StockIn)
                    .Sum(o => o.Quantity),
                TotalStockOut = p.Operations
                    .Where(o => o.Type == OperationType.StockOut)
                    .Sum(o => o.Quantity),
                CurrentStockLevel = p.CurrentStock
            })
            .AsNoTracking();
    }

    public async Task<DataGridResult<ProductViewModel>> GetStockLevelsAsync(
        DataGridRequest request, 
        DateTime? asOfDate = null, 
        CancellationToken cancellationToken = default)
    {
        IQueryable<ProductViewModel> query;

        if (asOfDate == null)
        {
            query = dbContext.Products
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    CurrentStock = p.CurrentStock,
                    MinimumStockLevel = p.MinimumStockLevel
                })
                .AsNoTracking();
        }
        else
        {
            query = await GetHistoricalStockQueryAsync(asOfDate.Value, cancellationToken);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{request.SearchTerm}%") ||
                EF.Functions.ILike(p.Code!, $"%{request.SearchTerm}%"));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDescending);

        // Apply pagination
        var items = await query
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new DataGridResult<ProductViewModel>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    private async Task<IQueryable<ProductViewModel>> GetHistoricalStockQueryAsync(
        DateTime osOfDate, CancellationToken cancellationToken)
    {
        var lastSnapshotDate = await dbContext.StockSnapshots
            .Where(s => s.SnapshotDate <= osOfDate.Date)
            .OrderByDescending(s => s.SnapshotDate)
            .Select(s => s.SnapshotDate)
            .FirstOrDefaultAsync(cancellationToken);

        var snapshots = await dbContext.StockSnapshots
            .Where(s => s.SnapshotDate == lastSnapshotDate)
            .AsNoTracking()
            .ToDictionaryAsync(s => s.ProductId, s => s.QuantityInStock, cancellationToken);

        var variances = await dbContext.Operations
            .Where(o => o.Date > lastSnapshotDate && o.Date <= osOfDate)
            .GroupBy(o => o.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Variance = g.Sum(o => o.Type == OperationType.StockIn ? o.Quantity : -o.Quantity)
            })
            .AsNoTracking()
            .ToDictionaryAsync(x => x.ProductId, x => x.Variance, cancellationToken);

        var productIds = snapshots.Keys.Union(variances.Keys).ToList();

        var query = dbContext.Products
            .Where(p => productIds.Contains(p.Id) || snapshots.Count == 0)
            .Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                CurrentStock = snapshots.GetValueOrDefault(p.Id, 0) + variances.GetValueOrDefault(p.Id, 0),
                MinimumStockLevel = p.MinimumStockLevel
            })
            .AsNoTracking();

        return query;
    }

    private IQueryable<ProductViewModel> ApplySorting(
        IQueryable<ProductViewModel> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderBy(p => p.Name);
        }

        return sortBy switch
        {
            nameof(ProductViewModel.Name) => sortDescending ? query.OrderByDescending(p => p.Name) 
                                                            : query.OrderBy(p => p.Name),
            nameof(ProductViewModel.Code) => sortDescending ? query.OrderByDescending(p => p.Code) 
                                                            : query.OrderBy(p => p.Code),
            nameof(ProductViewModel.CurrentStock) => sortDescending ? query.OrderByDescending(p => p.CurrentStock) 
                                                                    : query.OrderBy(p => p.CurrentStock),
            nameof(ProductViewModel.MinimumStockLevel) => sortDescending ? query.OrderByDescending(p => p.MinimumStockLevel)
                                                                         : query.OrderBy(p => p.MinimumStockLevel),
            _ => query.OrderBy(p => p.Name)
        };
    }
}
