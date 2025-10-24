using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Models;

namespace StockManagement.Services;

public interface IStockSnapshotService
{
    Task TakeWeeklySnapshotsAsync(CancellationToken cancellationToken = default);
}

internal class StockSnapshotService(
    IDbContextFactory<StockDbContext> dbContextFactory,
    ILogger<StockSnapshotService> logger) : IStockSnapshotService
{
    public async Task TakeWeeklySnapshotsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Démarrage du snapshot hebdomadaire à {Time}", DateTime.UtcNow);

            using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var snapshotDate = DateTime.UtcNow.Date;

            var existingSnapshotProductIds = await dbContext.StockSnapshots
                .Where(s => s.SnapshotDate == snapshotDate)
                .Select(s => s.ProductId)
                .ToListAsync(cancellationToken);

            var productsToSnapshot = await dbContext.Products
                .Where(p => !existingSnapshotProductIds.Contains(p.Id))
                .Select(p => new StockSnapshot
                {
                    ProductId = p.Id,
                    QuantityInStock = p.CurrentStock,
                    SnapshotDate = snapshotDate
                })
                .ToListAsync(cancellationToken);

            if (productsToSnapshot.Count != 0)
            {
                dbContext.StockSnapshots.AddRange(productsToSnapshot);
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Snapshot hebdomadaire terminé pour {Count} produits à {Time}",
                    productsToSnapshot.Count, DateTime.UtcNow);
            }
            else
            {
                logger.LogInformation("Aucun nouveau produit à capturer pour le snapshot hebdomadaire à {Time}",
                    DateTime.UtcNow);
            }
        }
        catch (Exception)
        {
            logger.LogError("Erreur lors de la prise du snapshot hebdomadaire à {Time}", DateTime.UtcNow);
            throw;
        }        
    }
}
