using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using StockManagement.Data;
using StockManagement.Exceptions;
using StockManagement.Mappers;
using StockManagement.Models;
using StockManagement.ViewModels.Inventories;

namespace StockManagement.Services;

public interface IInventoryService
{
    Task DeleteAsync(int id);
    IQueryable<InventoryViewModel> GetInventoriesListQuery();
    Task<List<InventoryRowViewModel>> GetRowsViewModelAsync(int? inventoryId = null);
    Task<InventoryViewModel?> GetInventoryDetailsAsync(int inventoryId);
    Task SaveAsDraftAsync(List<InventoryRowViewModel> viewModels);
    Task SaveAsCompletedAsync(List<InventoryRowViewModel> viewModels);
    Task UpdateAsync(int inventoryId, List<InventoryRowViewModel> viewModels, InventoryStatus? status = null);
}

internal class InventoryService(
    StockDbContext dbContext,
    ILogger<InventoryService> logger) : IInventoryService
{
    public async Task DeleteAsync(int id)
    {
        try
        {
            var inventory = await dbContext.Inventories.FindAsync(id);

            if (inventory == null)
                throw new NotFoundException("Inventory not found");

            dbContext.Inventories.Remove(inventory);
            await dbContext.SaveChangesAsync();
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting inventory {InventoryId}", id);
            throw new InternalServerException();
        }
    }

    public IQueryable<InventoryViewModel> GetInventoriesListQuery()
    {
        return dbContext.Inventories
            .OrderBy(i => i.Date)
            .Select(InventoryMappers.ToViewModelExpression())
            .AsNoTracking();
    }

    public Task<List<InventoryRowViewModel>> GetRowsViewModelAsync(int? inventoryId = null)
    {
        if (inventoryId == null)
        {
            return dbContext.Products
                .OrderBy(p => p.Id)
                .Select(p => new InventoryRowViewModel
                {
                    ProductId = p.Id,
                    ProductCode = p.Code,
                    ProductName = p.Name,
                    ExpectedQuantity = p.CurrentStock,
                    CountedQuantity = 0
                })
                .AsNoTracking()
                .ToListAsync();
        }
        else
        {
            return dbContext.InventoryRows
                .Where(r => r.InventoryId == inventoryId)
                .Select(r =>  new InventoryRowViewModel
                {
                    ProductId = r.ProductId,
                    ProductCode = r.Product!.Code,
                    ProductName = r.Product!.Name,
                    ExpectedQuantity = r.ExpectedQuantity,
                    CountedQuantity = r.CountedQuantity
                })
                .AsNoTracking()
                .ToListAsync();
        }            
    }

    public async Task<InventoryViewModel?> GetInventoryDetailsAsync(int inventoryId)
    {
        return await dbContext.Inventories
            .Where(i => i.Id == inventoryId)
            .Select(i => new InventoryViewModel
            {
                Id = i.Id,
                Code = i.Code,
                Date = i.Date,
                Status = i.Status,
                TotalExpectedUnits = i.Rows
                    .Sum(r => r.ExpectedQuantity),
                TotalCountedUnits = i.Rows
                    .Sum(r => r.CountedQuantity),
                Variance = i.Rows
                    .Sum(r => r.CountedQuantity - r.ExpectedQuantity)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task SaveAsDraftAsync(List<InventoryRowViewModel> viewModels)
    {
        await SaveAsync(viewModels, InventoryStatus.Pending);
    }

    public async Task SaveAsCompletedAsync(List<InventoryRowViewModel> viewModels)
    {
        await SaveAsync(viewModels, InventoryStatus.Completed);
    }

    public async Task UpdateAsync(int inventoryId, List<InventoryRowViewModel> viewModels, InventoryStatus? status = null)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            if (viewModels == null || viewModels.Count == 0)
                throw new ValidationException("Cannot update with empty inventory rows");

            var inventory = await dbContext.Inventories
                .Include(i => i.Rows)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                throw new NotFoundException("Inventory not found");

            if (inventory.Status != InventoryStatus.Pending)
                throw new ValidationException("Only pending inventories can be updated");

            if (status.HasValue)
                inventory.Status = status.Value;

            var viewModelsByProductId = viewModels.ToDictionary(r => r.ProductId);

            foreach (var row in inventory.Rows)
            {
                if (viewModelsByProductId.TryGetValue(row.ProductId, out var viewModel))
                {
                    row.CountedQuantity = viewModel.CountedQuantity;
                }
            }
            
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (BaseException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error while updating inventory {InventoryId}", inventoryId);
            throw new InternalServerException();
        }
    }

    private async Task SaveAsync(List<InventoryRowViewModel> viewModels, InventoryStatus status)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        
        try
        {
            if (viewModels == null || viewModels.Count == 0)
            {
                throw new ValidationException("Cannot save an empty inventory");
            }

            var inventory = new Inventory { Status = status };
            var rows = viewModels.ToModels();

            inventory.Rows.AddRange(rows);
            await dbContext.Inventories.AddAsync(inventory);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (BaseException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error while saving inventory with status {Status}", status);
            throw new InternalServerException();
        }
    }
}
