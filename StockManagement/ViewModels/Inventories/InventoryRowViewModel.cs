namespace StockManagement.ViewModels.Inventories;

public class InventoryRowViewModel
{
    public int ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ExpectedQuantity { get; set; }
    public int CountedQuantity { get; set; }
}
