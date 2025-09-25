using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagement.Models;

public class InventoryRow : AuditableEntity
{
    public required int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }
    public required int ProductId { get; set; }
    public Product? Product { get; set; }
    public required int ExpectedQuantity { get; set; }
    public required int CountedQuantity { get; set; }

    [NotMapped]
    public int Variation => CountedQuantity - ExpectedQuantity;
}
