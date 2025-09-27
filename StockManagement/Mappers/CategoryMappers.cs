using StockManagement.Models;
using StockManagement.ViewModels.Categories;

namespace StockManagement.Mappers;

public static class CategoryMappers
{
    public static Category ToModel(this CreateOrUpdateCategoryViewModel viewModel)
    {
        return new Category
        {
            Name = viewModel.Name,
            Description = viewModel.Description
        };
    }
}
