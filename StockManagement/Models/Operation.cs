namespace StockManagement.Models;

public abstract class Operation : AuditableEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public OperationType Type { get; protected set; }
}

public enum OperationType
{
    StockIn,
    StockOut
}