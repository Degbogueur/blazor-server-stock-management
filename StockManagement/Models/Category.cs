namespace StockManagement.Models;

public class Category : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}