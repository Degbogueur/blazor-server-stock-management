using StockManagement.Models;

namespace StockManagement.ViewModels.Operations;

public class OperationViewModel
{
    public int OperationId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public OperationType Type { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeFullName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OperationFiltersViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? ProductId { get; set; }
    public int? SupplierId { get; set; }
    public int? EmployeeId { get; set; }
    public OperationType? Type { get; set; }
}
