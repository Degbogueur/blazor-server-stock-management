using StockManagement.Models;
using StockManagement.ViewModels.Inventories;
using System.Linq.Expressions;

namespace StockManagement.Mappers;

public static class InventoryMappers
{
    public static List<InventoryRow> ToModels(this List<InventoryRowViewModel> viewModels)
    {
        return viewModels.Select(r => new InventoryRow
        {
            ProductId = r.ProductId,
            ExpectedQuantity = r.ExpectedQuantity,
            CountedQuantity = r.CountedQuantity
        }).ToList();
    }

    public static Expression<Func<Inventory, InventoryViewModel>> ToViewModelExpression()
    {
        return inventory => new InventoryViewModel
        {
            Id = inventory.Id,
            Code = inventory.Code,
            Date = inventory.Date,
            Status = inventory.Status,
            TotalExpectedUnits = inventory.Rows.Sum(r => r.ExpectedQuantity),
            TotalCountedUnits = inventory.Rows.Sum(r => r.CountedQuantity),
            Variance = inventory.Rows.Sum(r => r.CountedQuantity - r.ExpectedQuantity)
        }; 
    }
}
