using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Models;

namespace StockManagement.Data.Configurations;

public class CustomSettingConfiguration : IEntityTypeConfiguration<CustomSetting>
{
    public void Configure(EntityTypeBuilder<CustomSetting> builder)
    {
        builder.ToTable("Settings");

        builder.HasKey(s => s.Key);
    }
}
