using StockManagement.Models;
using StockManagement.ViewModels.Operations;

namespace StockManagement.Mappers;

public static class OperationMappers
{
    public static List<StockInOperation> ToStockInModels(this List<StockInOperationViewModel> viewModels)
    {
        return viewModels.Select(o => new StockInOperation
        {
            ProductId = o.ProductId,
            SupplierId = o.SupplierId,
            Quantity = o.Quantity,
            Date = o.Date
        }).ToList();
    }

    public static List<StockOutOperation> ToStockOutModels(this List<StockOutOperationViewModel> viewModels)
    {
        return viewModels.Select(o => new StockOutOperation
        {
            ProductId = o.ProductId,
            EmployeeId = o.EmployeeId,
            Quantity = o.Quantity,
            Date = o.Date
        }).ToList();
    }
}
