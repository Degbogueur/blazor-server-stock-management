using StockManagement.Models;

namespace StockManagement.ViewModels.Products;

public class StockCardViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public int CurrentStockLevel { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<StockCardRowViewModel> Rows { get; set; } = [];
    public int TotalRows { get; set; }
    public int InitialBalance { get; set; }
}

public class StockCardRowViewModel
{
    public DateTime Date { get; set; }
    public OperationType Type { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Balance { get; set; }
    public DateTime CreatedAt { get; set; }
}