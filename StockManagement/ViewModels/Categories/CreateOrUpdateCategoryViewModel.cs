using System.ComponentModel.DataAnnotations;

namespace StockManagement.ViewModels.Categories;

public class CreateOrUpdateCategoryViewModel
{
    public int? Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
