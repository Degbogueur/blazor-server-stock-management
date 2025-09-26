using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Models;

namespace StockManagement.Data.Configurations;

public class InventoryRowConfiguration : IEntityTypeConfiguration<InventoryRow>
{
    public void Configure(EntityTypeBuilder<InventoryRow> builder)
    {
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
