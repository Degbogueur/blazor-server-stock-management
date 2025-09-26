using Microsoft.EntityFrameworkCore;
using StockManagement.Data;

namespace StockManagement.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddDatabaseConnection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StockDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("StockDbConnection"));
        });
    }
}
