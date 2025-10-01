namespace StockManagement.ViewModels.Results;

public class SaveOperationResult
{
    public bool IsSuccess { get; set; }
    public List<StockAlertInfo> Alerts { get; set; } = [];
}

public class StockAlertInfo
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int NewStock { get; set; }
    public int MinimumStockLevel { get; set; }
    public bool IsOutOfStock => NewStock <= 0;
    public bool IsLowStock => NewStock > 0 && NewStock <= MinimumStockLevel;
}