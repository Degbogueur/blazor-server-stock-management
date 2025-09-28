using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockManagement.Data;
using StockManagement.Mappers;
using StockManagement.ViewModels.Products;

namespace StockManagement.Services;

public interface IProductService
{
    Task<bool> AddNewProductAsync(CreateOrUpdateProductViewModel viewModel);
    IQueryable<ProductViewModel> GetProductsListQueryAsync();
}

internal class ProductService(
    StockDbContext dbContext,
    ILogger<ProductService> logger) : IProductService
{
    public async Task<bool> AddNewProductAsync(CreateOrUpdateProductViewModel viewModel)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Name == viewModel.CategoryName);

            if (category is null)
            {
                var addCategoryResult = await dbContext.Categories.AddAsync(new Models.Category { Name = viewModel.CategoryName });
                await dbContext.SaveChangesAsync();
                category = addCategoryResult.Entity;
            }

            var product = viewModel.ToModel(category.Id);

            await dbContext.Products.AddAsync(product);
            var result = await dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return result > 0;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error while creating new category.");
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public IQueryable<ProductViewModel> GetProductsListQueryAsync()
    {
        return dbContext.Products
            .Select(p => new ProductViewModel
            {
                Name = p.Name,
                Code = p.Code,
                CurrentStock = p.CurrentStock,
                MinimumStockLevel = p.MinimumStockLevel,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                IsOutOfStock = p.IsOutOfStock
            }).AsQueryable();
    }
}
