using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockManagement.Components.Account;
using StockManagement.Data;
using StockManagement.Models;
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
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOperationService, OperationService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IReportPdfService, ReportPdfService>();
        services.AddScoped<IStockSnapshotService, StockSnapshotService>();
    }

    public static void AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddCascadingAuthenticationState();

        services.AddScoped<IdentityUserAccessor>();

        services.AddScoped<IdentityRedirectManager>();

        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
        })
        .AddEntityFrameworkStores<StockDbContext>()
        .AddDefaultTokenProviders();
    }

    public static void AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AdminUserCredentials>(configuration.GetSection("AdminUserCredentials"));
    }

    public static void AddClaimsPrincipalFactory(this IServiceCollection services)
    {
        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
    }

    public static void AddHangfireConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c =>
                c.UseNpgsqlConnection(configuration.GetConnectionString("StockDbConnection"))));
        
        services.AddHangfireServer();
    }
}
