namespace StockManagement.Models;

public class StockOutOperation : Operation
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public StockOutOperation() => Type = OperationType.StockOut;
}
