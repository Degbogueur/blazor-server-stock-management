using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagement.Models;

public class InventoryRow : AuditableEntity
{
    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int ExpectedQuantity { get; set; }
    public int CountedQuantity { get; set; }

    [NotMapped]
    public int Variation => CountedQuantity - ExpectedQuantity;
}
