using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Models;

namespace StockManagement.Data.Configurations;

public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.Name).HasMaxLength(100)
                                     .IsRequired();

        builder.HasMany(p => p.Operations)
               .WithOne(o => o.Product)
               .HasForeignKey(o => o.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.InventoryRows)
               .WithOne(r => r.Product)
               .HasForeignKey(r => r.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
