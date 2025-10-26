using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Models;

namespace StockManagement.Data.Configurations;

public class EmployeeConfiguration : BaseEntityConfiguration<Employee>
{
    public override void Configure(EntityTypeBuilder<Employee> builder)
    {
        base.Configure(builder);

        builder.HasMany(e => e.Operations)
               .WithOne(o => o.Employee)
               .HasForeignKey(o => o.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
