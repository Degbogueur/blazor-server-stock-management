using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Models;

namespace StockManagement.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasQueryFilter(p => !p.IsDeleted);

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
