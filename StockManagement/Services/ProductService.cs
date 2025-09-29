using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Exceptions;
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
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var sameNameExists = await dbContext.Products
                .AsNoTracking()
                .AnyAsync(p => EF.Functions.ILike(p.Name, viewModel.Name));

            if (sameNameExists)
                throw new UnauthorizedOperationException("A product with the same name already exists");

            var category = await categoryService.GetOrCreateCategoryAsync(viewModel.CategoryName);

            var product = viewModel.ToModel(category.Id);

            await dbContext.Products.AddAsync(product);
            var result = await dbContext.SaveChangesAsync();
            
            await transaction.CommitAsync();

            return result > 0;
        }
        catch (BaseException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error while adding new product '{ProductName}'", viewModel.Name);
            throw new InternalServerException();
        }
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            var rowsAffected = await dbContext.Products
                .Where(p => p.Id == id &&
                           !p.Operations.Any() &&
                           !p.InventoryRows.Any())
                .ExecuteUpdateAsync(p => p
                    .SetProperty(p => p.IsDeleted, true)
                    .SetProperty(p => p.DeletedOn, DateTime.UtcNow));

            if (rowsAffected > 0) return true;

            var productExists = await dbContext.Products.IgnoreQueryFilters().AnyAsync(p => p.Id == id);
            if (!productExists) throw new NotFoundException("Product not found");

            throw new UnauthorizedOperationException(
                "This product cannot be deleted: it has associated operation entries");
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while removing product with ID: {id}", id);
            throw new InternalServerException();
        }
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
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var validationResult = await dbContext.Products
                .Where(p => p.Id == viewModel.Id || EF.Functions.ILike(p.Name, viewModel.Name))
                .Select(p => new { p.Id, p.Name })
                .AsNoTracking()
                .ToListAsync();

            var productExists = validationResult.Any(p => p.Id == viewModel.Id);
            if (!productExists) throw new NotFoundException("Product not found");

            var sameNameExists = validationResult.Any(p =>
                string.Equals(p.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase) && p.Id != viewModel.Id);
            if (sameNameExists)
                throw new UnauthorizedOperationException("A product with the same name already exists");

            var category = await categoryService.GetOrCreateCategoryAsync(viewModel.CategoryName);

            var rowsAffected = await dbContext.Products
                .Where(p => p.Id == viewModel.Id)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(p => p.Name, viewModel.Name)
                    .SetProperty(p => p.Code, viewModel.Code)
                    .SetProperty(p => p.MinimumStockLevel, viewModel.MinimumStockLevel)
                    .SetProperty(p => p.CategoryId, category.Id));

            await transaction.CommitAsync();
            return rowsAffected > 0;
        }
        catch (BaseException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error while updating product with ID: {id}", viewModel.Id);
            throw new InternalServerException();
        }
    }
}
