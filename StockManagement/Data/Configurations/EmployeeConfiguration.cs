using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Models;

namespace StockManagement.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasMany(e => e.Operations)
               .WithOne(o => o.Employee)
               .HasForeignKey(o => o.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
