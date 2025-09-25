namespace StockManagement.Models;

public class Inventory : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public string? Notes { get; set; }
    public InventoryStatus Status { get; set; } = InventoryStatus.Pending;

    public ICollection<InventoryRow> Rows { get; set; } = [];
}

public enum InventoryStatus
{
    Pending,
    Completed,
    Cancelled
}