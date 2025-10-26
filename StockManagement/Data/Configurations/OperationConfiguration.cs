using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Models;

namespace StockManagement.Data.Configurations;

public class OperationConfiguration : AuditableEntityConfiguration<Operation>
{
    public override void Configure(EntityTypeBuilder<Operation> builder)
    {
        base.Configure(builder);

        builder.Property(o => o.Type).HasConversion<string>();

        builder.Property(o => o.Date).HasColumnType("date");
    }
}
