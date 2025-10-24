using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Models;

namespace StockManagement.Data.Configurations;

public class StockSnapshotConfiguration : IEntityTypeConfiguration<StockSnapshot>
{
    public void Configure(EntityTypeBuilder<StockSnapshot> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => new { s.ProductId, s.SnapshotDate }).IsUnique();

        builder.Property(s => s.SnapshotDate).HasColumnType("date");
    }
}
