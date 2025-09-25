using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagement.Models;

public class Product : BaseEntity
{
    public required string Name { get; set; }
    public string? Code { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStockLevel { get; set; }
    public required int CategoryId { get; set; }
    public Category? Category { get; set; }

    [NotMapped]
    public bool IsLowStock => CurrentStock <= MinimumStockLevel;
    [NotMapped]
    public bool IsOutOfStock => CurrentStock <= 0;

    public ICollection<InventoryRow> InventoryRows { get; set; } = [];
    public ICollection<Operation> Operations { get; set; } = [];
}
