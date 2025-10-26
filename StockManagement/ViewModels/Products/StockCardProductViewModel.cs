namespace StockManagement.ViewModels.Products;

public class StockCardProductViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public int TotalStockIn { get; set; }
    public int TotalStockOut { get; set; }
    public int CurrentStockLevel { get; set; }
}
