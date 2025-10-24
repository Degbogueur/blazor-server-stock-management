namespace StockManagement.Models;

public class StockSnapshot
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public DateTime SnapshotDate { get; set; }
    public int QuantityInStock { get; set; }
}
