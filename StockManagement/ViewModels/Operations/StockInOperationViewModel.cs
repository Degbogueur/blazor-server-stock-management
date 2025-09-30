using System.ComponentModel.DataAnnotations;

namespace StockManagement.ViewModels.Operations;

public class StockInOperationViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "You cannot add less than one item")]
    public int Quantity { get; set; } = 1;
    public DateTime Date { get; set; }
}
