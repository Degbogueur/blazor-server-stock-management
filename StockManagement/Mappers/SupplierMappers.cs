using StockManagement.Models;
using StockManagement.ViewModels.Suppliers;

namespace StockManagement.Mappers;

public static class SupplierMappers
{
    public static Supplier ToModel(this CreateOrUpdateSupplierViewModel viewModel)
    {
        return new Supplier
        {
            Name = viewModel.Name,
            PhoneNumber = viewModel.PhoneNumber,
            Email = viewModel.Email,
            Address = viewModel.Address
        };
    }
}
