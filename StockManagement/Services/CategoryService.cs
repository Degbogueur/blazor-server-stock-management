using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Mappers;
using StockManagement.Models;
using StockManagement.ViewModels.Categories;

namespace StockManagement.Services;

public interface ICategoryService
{
    Task<bool> CreateNewCategoryAsync(CreateOrUpdateCategoryViewModel viewModel);
    Task<bool> DeleteCategoryAsync(int id);
    Task<IEnumerable<CategoryViewModel>> GetCategoriesListAsync();
    Task<Category> GetCategoryByNameOrCreateAsync(string name);
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
            var category = viewModel.ToModel();

            await dbContext.Categories.AddAsync(category);
            var result = await dbContext.SaveChangesAsync();

            return result > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while creating new category.");
            throw;
        }        
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        try
        {
            var isDeleted = await dbContext.Categories
                .Where(c => c.Id == id)
                .ExecuteUpdateAsync(c => c.SetProperty(c => c.IsDeleted, true));

            return isDeleted > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro while deleting category with ID: {id}", id);
            throw;
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

    public async Task<Category> GetCategoryByNameOrCreateAsync(string name)
    {
        var category = await dbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name);

        if (category is null)
        { 
            category = new Category { Name = name };
            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();
        }

        return category;
    }

    public async Task<bool> UpdateCategoryAsync(CreateOrUpdateCategoryViewModel viewModel)
    {
        try
        {
            var isUpdated = await dbContext.Categories
                .Where(c => c.Id == viewModel.Id)
                .ExecuteUpdateAsync(c => c
                    .SetProperty(c => c.Name, viewModel.Name)
                    .SetProperty(c => c.Description, viewModel.Description));

            return isUpdated > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating category with ID: {id}", viewModel.Id);
            throw;
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