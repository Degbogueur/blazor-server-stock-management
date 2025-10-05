namespace StockManagement.Models;

public class Inventory : AuditableEntity
{
    public string Code { get; set; }
    public DateTime Date { get; set; }
    public InventoryStatus Status { get; set; } = InventoryStatus.Pending;

    public List<InventoryRow> Rows { get; set; } = [];

    public Inventory()
    {
        Code = $"INV-{DateTime.UtcNow:dd-MM-yy-HH-mm-ss}";
        Date = DateTime.UtcNow.Date;
    }
}

public enum InventoryStatus
{
    Pending,
    Completed,
    Cancelled
}