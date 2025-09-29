namespace StockManagement.Models;

public class Supplier : BaseEntity
{
    public required string Name { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    public ICollection<StockInOperation> Operations { get; set; } = [];
}
