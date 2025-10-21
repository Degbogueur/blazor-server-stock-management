namespace StockManagement.Models;

public class Supplier : BaseEntity
{
    public required string Name { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }

    public ICollection<StockInOperation> Operations { get; set; } = [];
}
