using StockManagement.Models;
using StockManagement.ViewModels.Products;

namespace StockManagement.Mappers;

public static class ProductMappers
{
    public static Product ToModel(this CreateOrUpdateProductViewModel viewModel, int categoryId)
    {
        return new Product
        {
            Name = viewModel.Name,
            Code = viewModel.Code,
            MinimumStockLevel = viewModel.MinimumStockLevel,
            CategoryId = categoryId
        };
    }
}
