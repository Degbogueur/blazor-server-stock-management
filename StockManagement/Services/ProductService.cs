using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Mappers;
using StockManagement.ViewModels.Products;

namespace StockManagement.Services;

public interface IProductService
{
    Task<bool> AddNewProductAsync(CreateOrUpdateProductViewModel viewModel);
    Task<bool> DeleteProductAsync(int id);
    IQueryable<ProductViewModel> GetProductsListQueryAsync();
    Task<bool> UpdateProductAsync(CreateOrUpdateProductViewModel viewModel);
}

internal class ProductService(
    StockDbContext dbContext,
    ILogger<ProductService> logger,
    ICategoryService categoryService) : IProductService
{
    public async Task<bool> AddNewProductAsync(CreateOrUpdateProductViewModel viewModel)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var category = await categoryService.GetCategoryByNameOrCreateAsync(viewModel.CategoryName);

            var product = viewModel.ToModel(category.Id);

            await dbContext.Products.AddAsync(product);
            var result = await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return result > 0;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error while adding new product.");
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var isDeleted = await dbContext.Products
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(p => p
                .SetProperty(p => p.IsDeleted, true));

        return isDeleted > 0;
    }

    public IQueryable<ProductViewModel> GetProductsListQueryAsync()
    {
        return dbContext.Products
            .OrderBy(p => p.Id)
            .Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                CurrentStock = p.CurrentStock,
                MinimumStockLevel = p.MinimumStockLevel,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty
            })
            .AsNoTracking()
            .AsQueryable();
    }

    public async Task<bool> UpdateProductAsync(CreateOrUpdateProductViewModel viewModel)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var category = await categoryService.GetCategoryByNameOrCreateAsync(viewModel.CategoryName);

            var isUpdated = await dbContext.Products
                .Where(p => p.Id == viewModel.Id)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(p => p.Name, viewModel.Name)
                    .SetProperty(p => p.Code, viewModel.Code)
                    .SetProperty(p => p.MinimumStockLevel, viewModel.MinimumStockLevel)
                    .SetProperty(p => p.CategoryId, category.Id));

            await transaction.CommitAsync();
            return isUpdated > 0;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error while updating product.");
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }
}
