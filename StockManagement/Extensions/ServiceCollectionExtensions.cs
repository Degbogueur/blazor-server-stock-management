using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Services;

namespace StockManagement.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddDatabaseConnection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StockDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("StockDbConnection"));
        });

        services.AddDbContextFactory<StockDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("StockDbConnection"));
        }, ServiceLifetime.Scoped);
    }

    public static void AddDependencyInjectionContainer(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOperationService, OperationService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISupplierService, SupplierService>();
    }
}
