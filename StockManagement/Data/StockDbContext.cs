using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockManagement.Extensions;
using StockManagement.Models;
using System.Reflection;

namespace StockManagement.Data;

public class StockDbContext : IdentityDbContext<ApplicationUser>
{
    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureIdentitySchema();
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<CustomSetting> Settings { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryRow> InventoryRows { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Operation> Operations { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
}
