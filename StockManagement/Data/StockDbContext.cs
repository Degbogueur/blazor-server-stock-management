using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockManagement.Extensions;
using StockManagement.Models;
using StockManagement.Services;
using System.Reflection;

namespace StockManagement.Data;

public class StockDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentUserService currentUserService;
    public StockDbContext(
        DbContextOptions<StockDbContext> options,
        ICurrentUserService currentUserService
        ) : base(options) 
    {
        this.currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureIdentitySchema();
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditableEntityProperties();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetAuditableEntityProperties();
        return base.SaveChanges();
    }

    private void SetAuditableEntityProperties()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || 
                        e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted);

        var currentUserId = currentUserService.UserId ?? "System";
        var timestamp = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Deleted && entry.Entity is BaseEntity baseEntity)
            {
                entry.State = EntityState.Modified;
                baseEntity.IsDeleted = true;
                baseEntity.DeletedAt = timestamp;
                baseEntity.DeletedById = currentUserId;
            }

            if (entry.Entity is AuditableEntity auditableEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditableEntity.CreatedAt = timestamp;
                        auditableEntity.CreatedById = currentUserId;
                        auditableEntity.UpdatedAt = timestamp;
                        auditableEntity.UpdatedById = currentUserId;
                        break;
                    case EntityState.Modified:
                        auditableEntity.UpdatedAt = timestamp;
                        auditableEntity.UpdatedById = currentUserId;
                        break;
                }
            }
        }
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
    public DbSet<StockSnapshot> StockSnapshots { get; set; }
    }
