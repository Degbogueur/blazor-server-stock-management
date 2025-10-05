using StockManagement.Models;

namespace StockManagement.ViewModels.Inventories;

public class InventoryViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public InventoryStatus Status { get; set; }
    public int TotalProducts { get; set; }
    public int MatchingCount { get; set; }
    public int DiscrepanciesCount { get; set; }
    public int Variance { get; set; }
}
