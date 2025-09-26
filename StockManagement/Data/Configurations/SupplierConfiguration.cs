using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Models;

namespace StockManagement.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.Property(s => s.Name).HasMaxLength(200)
                                     .IsRequired();

        builder.HasMany(s => s.Operations)
               .WithOne(o => o.Supplier)
               .HasForeignKey(o => o.SupplierId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
