using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Models;

namespace StockManagement.Data.Configurations;

public class InventoryConfiguration : AuditableEntityConfiguration<Inventory>
{
    public override void Configure(EntityTypeBuilder<Inventory> builder)
    {
        base.Configure(builder);

        builder.Property(i => i.Code).IsRequired();

        builder.Property(i => i.Date).HasColumnType("date");

        builder.Property(i => i.Status).HasConversion<string>();

        builder.HasMany(i => i.Rows)
               .WithOne(r => r.Inventory)
               .HasForeignKey(r => r.InventoryId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
