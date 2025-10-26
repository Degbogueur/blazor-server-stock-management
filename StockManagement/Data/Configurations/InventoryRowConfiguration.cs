using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Models;

namespace StockManagement.Data.Configurations;

public class InventoryRowConfiguration : AuditableEntityConfiguration<InventoryRow>
{
    public override void Configure(EntityTypeBuilder<InventoryRow> builder)
    {
        base.Configure(builder);
    }
}
