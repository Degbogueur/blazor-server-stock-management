namespace StockManagement.Models;

public class StockInOperation : Operation
{
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public StockInOperation() => Type = OperationType.StockIn;
}
