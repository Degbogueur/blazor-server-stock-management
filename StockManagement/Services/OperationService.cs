using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Exceptions;
using StockManagement.Mappers;
using StockManagement.ViewModels.Operations;
using StockManagement.ViewModels.Results;

namespace StockManagement.Services;

public interface IOperationService
{
    Task<bool> SaveStockInOperationsAsync(List<StockInOperationViewModel> viewModels);
    Task<SaveOperationResult> SaveStockOutOperationsAsync(List<StockOutOperationViewModel> viewModels);
}

internal class OperationService(
    StockDbContext dbContext,
    ILogger<OperationService> logger) : IOperationService
{
    public async Task<bool> SaveStockInOperationsAsync(List<StockInOperationViewModel> viewModels)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var stockInOperations = viewModels.ToStockInModels();

            await dbContext.Operations.AddRangeAsync(stockInOperations);
            await dbContext.SaveChangesAsync();

            var productUpdates = viewModels
                .GroupBy(o => o.ProductId)
                .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(o => o.Quantity) })
                .ToList();

            foreach (var update in productUpdates)
            {
                await dbContext.Products
                    .Where(p => p.Id == update.ProductId)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(p => p.CurrentStock, p => p.CurrentStock + update.TotalQuantity));
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (BaseException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error while saving stock entry operations");
            throw new InternalServerException();
        }
    }

    public async Task<SaveOperationResult> SaveStockOutOperationsAsync(List<StockOutOperationViewModel> viewModels)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var result = new SaveOperationResult { IsSuccess = true };

            var stockOutOperations = viewModels.ToStockOutModels();

            await dbContext.Operations.AddRangeAsync(stockOutOperations);
            await dbContext.SaveChangesAsync();

            var productUpdates = viewModels
                .GroupBy(o => o.ProductId)
                .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(o => o.Quantity) })
                .ToList();

            var productIds = productUpdates.Select(u => u.ProductId).ToList();
            var products = await dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Name, p.CurrentStock, p.MinimumStockLevel })
                .ToListAsync();

            foreach (var update in productUpdates)
            {
                await dbContext.Products
                    .Where(p => p.Id == update.ProductId)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(p => p.CurrentStock, p => p.CurrentStock - update.TotalQuantity));

                var product = products.First(p => p.Id == update.ProductId);
                var newStock = product.CurrentStock - update.TotalQuantity;

                if (newStock <= product.MinimumStockLevel)
                {
                    result.Alerts.Add(new StockAlertInfo
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        NewStock = newStock,
                        MinimumStockLevel = product.MinimumStockLevel
                    });
                }
            }

            await transaction.CommitAsync();
            return result;
        }
        catch (BaseException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error while saving stock withdrawal operations");
            throw new InternalServerException();
        }
    }
}
