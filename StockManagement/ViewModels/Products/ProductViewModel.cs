namespace StockManagement.ViewModels.Products;

public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; init; } = string.Empty;
    public string? Code { get; init; }
    public int CurrentStock { get; init; }
    public int MinimumStockLevel { get; init; }
    public string CategoryName { get; init; } = string.Empty;
}
