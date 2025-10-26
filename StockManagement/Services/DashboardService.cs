using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Models;

namespace StockManagement.Services;

public interface IDashboardService
{
    Task<DashboardStatistics> GetDashboardStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<StockMovementChartData>> GetStockMovementChartDataAsync(int days = 30);
    Task<List<CategoryStockData>> GetStockByCategoryAsync();
    Task<List<OperationsByDayData>> GetOperationsByDayOfWeekAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TopItemData>> GetTopProductsByOperationsAsync(int topCount = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TopItemData>> GetTopEmployeesAsync(int topCount = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TopItemData>> GetTopSuppliersAsync(int topCount = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<LowStockAlertData>> GetLowStockAlertsAsync();
}

internal class DashboardService(StockDbContext dbContext) : IDashboardService
{
    public async Task<DashboardStatistics> GetDashboardStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddMonths(-1);
        var end = endDate ?? DateTime.Today;

        var totalProducts = await dbContext.Products
            .CountAsync();

        var totalStockValue = await dbContext.Products
            .SumAsync(p => p.CurrentStock);

        var lowStockCount = await dbContext.Products
            .Where(p => p.CurrentStock <= p.MinimumStockLevel && p.CurrentStock > 0)
            .CountAsync();

        var outOfStockCount = await dbContext.Products
            .Where(p => p.CurrentStock <= 0)
            .CountAsync();

        var totalStockIn = await dbContext.Set<StockInOperation>()
            .Where(o => o.Date >= start && o.Date <= end)
            .SumAsync(o => (int?)o.Quantity) ?? 0;

        var totalStockOut = await dbContext.Set<StockOutOperation>()
            .Where(o => o.Date >= start && o.Date <= end)
            .SumAsync(o => (int?)o.Quantity) ?? 0;

        var totalOperations = await dbContext.Set<StockInOperation>()
            .Where(o => o.Date >= start && o.Date <= end)
            .CountAsync() +
            await dbContext.Set<StockOutOperation>()
            .Where(o => o.Date >= start && o.Date <= end)
            .CountAsync();

        var pendingInventories = await dbContext.Inventories
            .Where(i => i.Status == InventoryStatus.Pending)
            .CountAsync();

        return new DashboardStatistics
        {
            TotalProducts = totalProducts,
            TotalStockValue = totalStockValue,
            LowStockCount = lowStockCount,
            OutOfStockCount = outOfStockCount,
            TotalStockIn = totalStockIn,
            TotalStockOut = totalStockOut,
            TotalOperations = totalOperations,
            PendingInventories = pendingInventories,
            PeriodStart = start,
            PeriodEnd = end
        };
    }

    public async Task<List<StockMovementChartData>> GetStockMovementChartDataAsync(int days = 30)
    {
        var startDate = DateTime.Today.AddDays(-days);
        var endDate = DateTime.Today;

        var stockInByDate = await dbContext.Set<StockInOperation>()
            .Where(o => o.Date >= startDate && o.Date <= endDate)
            .GroupBy(o => o.Date.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(o => o.Quantity) })
            .ToListAsync();

        var stockOutByDate = await dbContext.Set<StockOutOperation>()
            .Where(o => o.Date >= startDate && o.Date <= endDate)
            .GroupBy(o => o.Date.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(o => o.Quantity) })
            .ToListAsync();

        var allDates = Enumerable.Range(0, days + 1)
            .Select(d => startDate.AddDays(d).Date)
            .ToList();

        return allDates.Select(date => new StockMovementChartData
        {
            Date = date,
            StockIn = stockInByDate.FirstOrDefault(x => x.Date == date)?.Total ?? 0,
            StockOut = stockOutByDate.FirstOrDefault(x => x.Date == date)?.Total ?? 0
        }).ToList();
    }

    public async Task<List<CategoryStockData>> GetStockByCategoryAsync()
    {
        return await dbContext.Categories
            .Select(c => new CategoryStockData
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                TotalStock = c.Products
                    .Sum(p => p.CurrentStock),
                ProductCount = c.Products
                    .Count()
            })
            .Where(c => c.ProductCount > 0)
            .OrderByDescending(c => c.TotalStock)
            .ToListAsync();
    }

    public async Task<List<OperationsByDayData>> GetOperationsByDayOfWeekAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddMonths(-1);
        var end = endDate ?? DateTime.Today;

        var stockInOps = await dbContext.Set<StockInOperation>()
            .Where(o => o.Date >= start && o.Date <= end)
            .Select(o => o.Date.DayOfWeek)
            .ToListAsync();

        var stockOutOps = await dbContext.Set<StockOutOperation>()
            .Where(o => o.Date >= start && o.Date <= end)
            .Select(o => o.Date.DayOfWeek)
            .ToListAsync();

        var allOps = stockInOps.Concat(stockOutOps).ToList();

        var dayOrder = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 0 },
            { DayOfWeek.Tuesday, 1 },
            { DayOfWeek.Wednesday, 2 },
            { DayOfWeek.Thursday, 3 },
            { DayOfWeek.Friday, 4 },
            { DayOfWeek.Saturday, 5 },
            { DayOfWeek.Sunday, 6 }
        };

        return allOps
            .GroupBy(d => d)
            .Select(g => new OperationsByDayData
            {
                DayOfWeek = g.Key.ToString(),
                TotalOperations = g.Count(),
                Order = dayOrder[g.Key]
            })
            .OrderBy(x => x.Order)
            .ToList();
    }

    public async Task<List<TopItemData>> GetTopProductsByOperationsAsync(int topCount = 5, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddMonths(-1);
        var end = endDate ?? DateTime.Today;

        var stockInOps = await dbContext.Set<StockInOperation>()
            .Where(o => o.Date >= start && o.Date <= end)
            .Select(o => new { o.ProductId, o.Product!.Name })
            .ToListAsync();

        var stockOutOps = await dbContext.Set<StockOutOperation>()
            .Where(o => o.Date >= start && o.Date <= end)
            .Select(o => new { o.ProductId, o.Product!.Name })
            .ToListAsync();

        return stockInOps.Concat(stockOutOps)
            .GroupBy(o => new { o.ProductId, o.Name })
            .Select(g => new TopItemData
            {
                Id = g.Key.ProductId,
                Name = g.Key.Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(topCount)
            .ToList();
    }

    public async Task<List<TopItemData>> GetTopEmployeesAsync(int topCount = 5, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddMonths(-1);
        var end = endDate ?? DateTime.Today;

        return await dbContext.Set<StockOutOperation>()
            .Where(o => o.Date >= start && o.Date <= end)
            .GroupBy(o => new { o.EmployeeId, o.Employee!.FirstName, o.Employee.LastName })
            .Select(g => new TopItemData
            {
                Id = g.Key.EmployeeId,
                Name = g.Key.FirstName + " " + g.Key.LastName,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<List<TopItemData>> GetTopSuppliersAsync(int topCount = 5, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddMonths(-1);
        var end = endDate ?? DateTime.Today;

        return await dbContext.Set<StockInOperation>()
            .Where(o => o.Date >= start && o.Date <= end)
            .GroupBy(o => new { o.SupplierId, o.Supplier!.Name })
            .Select(g => new TopItemData
            {
                Id = g.Key.SupplierId,
                Name = g.Key.Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<List<LowStockAlertData>> GetLowStockAlertsAsync()
    {
        return await dbContext.Products
            .Where(p => p.CurrentStock <= p.MinimumStockLevel)
            .OrderBy(p => p.CurrentStock)
            .Select(p => new LowStockAlertData
            {
                ProductId = p.Id,
                ProductName = p.Name,
                ProductCode = p.Code,
                CurrentStock = p.CurrentStock,
                MinimumStockLevel = p.MinimumStockLevel,
                IsOutOfStock = p.CurrentStock <= 0
            })
            .Take(5)
            .ToListAsync();
    }
}


// View Models
public class DashboardStatistics
{
    public int TotalProducts { get; set; }
    public int TotalStockValue { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int TotalStockIn { get; set; }
    public int TotalStockOut { get; set; }
    public int TotalOperations { get; set; }
    public int PendingInventories { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class StockMovementChartData
{
    public DateTime Date { get; set; }
    public int StockIn { get; set; }
    public int StockOut { get; set; }
}

public class TopItemData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class CategoryStockData
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalStock { get; set; }
    public int ProductCount { get; set; }
}

public class OperationsByDayData
{
    public string DayOfWeek { get; set; } = string.Empty;
    public int TotalOperations { get; set; }
    public int Order { get; set; }
}

public class LowStockAlertData
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStockLevel { get; set; }
    public bool IsOutOfStock { get; set; }
}