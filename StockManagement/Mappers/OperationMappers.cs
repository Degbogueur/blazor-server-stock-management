using StockManagement.Models;
using StockManagement.ViewModels.Operations;

namespace StockManagement.Mappers;

public static class OperationMappers
{
    public static List<StockInOperation> ToModels(this List<StockInOperationViewModel> viewModels)
    {
        return viewModels.Select(o => new StockInOperation
        {
            ProductId = o.ProductId,
            SupplierId = o.SupplierId,
            Quantity = o.Quantity,
            Date = o.Date
        }).ToList();
    }
}
