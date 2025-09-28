using System.ComponentModel.DataAnnotations;

namespace StockManagement.ViewModels.Products;

public class CreateOrUpdateProductViewModel
{
    [Required] 
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    [Required, Range(0, int.MaxValue, ErrorMessage = "The minimum stock level cannot be less than 0")]
    public int MinimumStockLevel { get; set; } = 0;
    [Required(ErrorMessage = "The category is required")]
    public string CategoryName { get; set; } = string.Empty;
}
