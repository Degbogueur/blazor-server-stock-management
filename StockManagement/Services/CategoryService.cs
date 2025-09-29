using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Exceptions;
using StockManagement.Mappers;
using StockManagement.Models;
using StockManagement.ViewModels.Categories;

namespace StockManagement.Services;

public interface ICategoryService
{
    Task<bool> CreateNewCategoryAsync(CreateOrUpdateCategoryViewModel viewModel);
    Task<bool> DeleteCategoryAsync(int id);
    Task<IEnumerable<CategoryViewModel>> GetCategoriesListAsync();
    Task<Category> GetOrCreateCategoryAsync(string name);
    Task<bool> UpdateCategoryAsync(CreateOrUpdateCategoryViewModel viewModel);
    Task<IEnumerable<string>> SearchCategoriesAsync(string value, CancellationToken token, int count = 10);
}

internal class CategoryService(
    StockDbContext dbContext,
    ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<bool> CreateNewCategoryAsync(CreateOrUpdateCategoryViewModel viewModel)
    {
        try
        {
            var sameNameExists = await dbContext.Categories
                .AsNoTracking()
                .AnyAsync(c => EF.Functions.ILike(c.Name, viewModel.Name));

            if (sameNameExists)
                throw new UnauthorizedOperationException("A category with the same name already exists");

            var category = viewModel.ToModel();

            await dbContext.Categories.AddAsync(category);
            var result = await dbContext.SaveChangesAsync();

            return result > 0;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while creating new category '{CategoryName}'.", viewModel.Name);
            throw new InternalServerException();
        }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        try
        {
            var rowsAffected = await dbContext.Categories
                .Where(c => c.Id == id &&
                           !c.Products.Any())
                .ExecuteUpdateAsync(c => c
                .SetProperty(c => c.IsDeleted, true)
                .SetProperty(c => c.DeletedOn, DateTime.UtcNow));

            if (rowsAffected > 0) return true;

            var categoryExists = await dbContext.Categories.IgnoreQueryFilters().AnyAsync(c => c.Id == id);
            if (!categoryExists) throw new NotFoundException("Category not found");

            throw new UnauthorizedOperationException(
                "This category cannot be deleted: it has associated products");
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro while deleting category with ID: {id}", id);
            throw new InternalServerException();
        }
    }

    public async Task<IEnumerable<CategoryViewModel>> GetCategoriesListAsync()
    {
        return await dbContext.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductsCount = c.Products.Count
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Category> GetOrCreateCategoryAsync(string name)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Name == name);

        if (category == null)
        { 
            category = new Category { Name = name };
            await dbContext.Categories.AddAsync(category);
        }

        return category;
    }

    public async Task<bool> UpdateCategoryAsync(CreateOrUpdateCategoryViewModel viewModel)
    {
        try
        {
            var validationResult = await dbContext.Categories
                .Where(c => c.Id == viewModel.Id || EF.Functions.ILike(c.Name, viewModel.Name))
                .Select(p => new { p.Id, p.Name })
                .AsNoTracking()
                .ToListAsync();

            var categoryExists = validationResult.Any(c => c.Id == viewModel.Id);
            if (!categoryExists) throw new NotFoundException("Category not found");

            var sameNameExists = validationResult.Any(c =>
                string.Equals(c.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase) && c.Id != viewModel.Id);
            if (sameNameExists)
                throw new UnauthorizedOperationException("A category with the same name already exists");

            var rowsAffected = await dbContext.Categories
                .Where(c => c.Id == viewModel.Id)
                .ExecuteUpdateAsync(c => c
                    .SetProperty(c => c.Name, viewModel.Name)
                    .SetProperty(c => c.Description, viewModel.Description));

            return rowsAffected > 0;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating category with ID: {id}", viewModel.Id);
            throw new InternalServerException();
        }
    }

    public async Task<IEnumerable<string>> SearchCategoriesAsync(string value, CancellationToken token = default, int count = 10)
    {
        return await dbContext.Categories
            .Where(c => EF.Functions.ILike(c.Name, $"%{value}%"))
            .OrderBy(c => c.Name)
            .Take(count)
            .Select(c => c.Name)
            .AsNoTracking()
            .ToListAsync(token);
    }
}