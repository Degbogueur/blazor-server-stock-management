using System.ComponentModel.DataAnnotations;

namespace StockManagement.ViewModels.Operations;

public class StockOutOperationViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
    public DateTime Date { get; set; }
}
