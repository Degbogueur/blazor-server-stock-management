namespace StockManagement.Models;

public class Operation : AuditableEntity
{
    public required int ProductId { get; set; }
    public Product? Product { get; set; }
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public OperationType Type { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
}

public enum OperationType
{
    StockIn,
    StockOut
};