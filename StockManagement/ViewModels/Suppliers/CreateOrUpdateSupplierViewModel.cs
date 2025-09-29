using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StockManagement.ViewModels.Suppliers;

public class CreateOrUpdateSupplierViewModel
{
    public int? Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required, DisplayName("Phone number")] public string PhoneNumber { get; set; } = string.Empty;
    [EmailAddress(ErrorMessage = "The email address is invalid")] public string? Email { get; set; }
    public string? Address { get; set; }
}
