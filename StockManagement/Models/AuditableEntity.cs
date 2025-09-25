namespace StockManagement.Models;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime? CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastUpdatedOn { get; set; }
    public string? LastUpdatedBy { get; set; }
}
