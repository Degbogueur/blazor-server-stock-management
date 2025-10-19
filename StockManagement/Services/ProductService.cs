using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Exceptions;
using StockManagement.Mappers;
using StockManagement.ViewModels;
using StockManagement.ViewModels.Products;

namespace StockManagement.Services;

public interface IProductService
{
    Task<bool> AddNewProductAsync(CreateOrUpdateProductViewModel viewModel);
    Task<bool> DeleteProductAsync(int id);
    IQueryable<ProductViewModel> GetProductsListQuery();
    Task<List<SearchProductResultViewModel>> SearchProductsAsync(string term, CancellationToken token, int maxResults = 10);
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
            await dbContext.SaveChangesAsync();

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
            var product = await dbContext.Products
                .Include(p => p.Operations)
                .Include(p => p.InventoryRows)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new NotFoundException("Product not found");

            if (product.Operations.Count != 0 || product.InventoryRows.Count != 0)
                throw new UnauthorizedOperationException(
                    "This product cannot be deleted: it has associated operation entries");

            dbContext.Products.Remove(product);
            var result = await dbContext.SaveChangesAsync();

            return result > 0;
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

    public IQueryable<ProductViewModel> GetProductsListQuery()
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

    public async Task<List<SearchProductResultViewModel>> SearchProductsAsync(
        string term, CancellationToken token = default, int maxResults = 10)
    {
        return await dbContext.Products
            .Where(p => EF.Functions.ILike(p.Name, $"%{term}%"))
            .OrderBy(p => p.Name)
            .Take(maxResults)
            .Select(p => new SearchProductResultViewModel { Id = p.Id, Text = p.Name, Quantity = p.CurrentStock })
            .AsNoTracking()
            .ToListAsync(token);
    }

    public async Task<bool> UpdateProductAsync(CreateOrUpdateProductViewModel viewModel)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var sameNameExists = await dbContext.Products
                .AsNoTracking()
                .AnyAsync(p => EF.Functions.ILike(p.Name, viewModel.Name) && p.Id != viewModel.Id);

            if (sameNameExists)
                throw new UnauthorizedOperationException("A product with the same name already exists");

            var product = await dbContext.Products.FindAsync(viewModel.Id);

            if (product == null)
                throw new NotFoundException("Product not found");

            var category = await categoryService.GetOrCreateCategoryAsync(viewModel.CategoryName);
            await dbContext.SaveChangesAsync();

            product.Name = viewModel.Name;
            product.Code = viewModel.Code;
            product.MinimumStockLevel = viewModel.MinimumStockLevel;
            product.CategoryId = category.Id;

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
            logger.LogError(ex, "Error while updating product with ID: {id}", viewModel.Id);
            throw new InternalServerException();
        }
    }
}
