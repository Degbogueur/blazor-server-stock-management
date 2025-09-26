using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Models;

namespace StockManagement.Data.Configurations;

public class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.HasQueryFilter(o => !o.IsDeleted);

        builder.Property(o => o.Type).HasConversion<string>();

        builder.Property(o => o.Date).HasColumnType("date");
    }
}
