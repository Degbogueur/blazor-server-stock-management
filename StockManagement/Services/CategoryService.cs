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
    Task<IEnumerable<string>> SearchCategorieNamesAsync(string term, CancellationToken token, int maxResults = 10);
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
            var category = await dbContext.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                throw new NotFoundException("Category not found");

            if (category.Products.Count != 0)
                throw new UnauthorizedOperationException(
                    "This category cannot be deleted: it has associated products");

            dbContext.Categories.Remove(category);
            var result = await dbContext.SaveChangesAsync();

            return result > 0;
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
        string normalizedName = name.Trim();
        var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Name == normalizedName);

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
            var sameNameExists = await dbContext.Categories
                .AsNoTracking()
                .AnyAsync(c => EF.Functions.ILike(c.Name, viewModel.Name) && c.Id != viewModel.Id);

            if (sameNameExists)
                throw new UnauthorizedOperationException("A category with the same name already exists");

            var category = await dbContext.Categories.FindAsync(viewModel.Id);
            
            if (category == null)
                throw new NotFoundException("Category not found");

            category.Name = viewModel.Name;
            category.Description = viewModel.Description;

            var result = await dbContext.SaveChangesAsync();

            return result > 0;
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

    public async Task<IEnumerable<string>> SearchCategorieNamesAsync(
        string term, CancellationToken token = default, int maxResults = 10)
    {
        return await dbContext.Categories
            .Where(c => EF.Functions.ILike(c.Name, $"%{term}%"))
            .OrderBy(c => c.Name)
            .Take(maxResults)
            .Select(c => c.Name)
            .AsNoTracking()
            .ToListAsync(token);
    }
}