using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StockManagement.ViewModels.Employees;

public class CreateOrUpdateEmployeeViewModel
{
    public int? Id { get; set; }
    [Required, DisplayName("First name")]
    public string FirstName { get; set; } = string.Empty;
    [Required, DisplayName("Last name")]
    public string LastName { get; set; } = string.Empty;
    public string? Position { get; set; }
}