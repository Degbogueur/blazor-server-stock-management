using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Exceptions;
using StockManagement.Models;
using StockManagement.ViewModels.Results;

namespace StockManagement.Services;

public interface INotificationService
{
    event Action? OnNotificationChanged;

    Task AddNotificationAsync(string title, string message, NotificationType type = NotificationType.Info);
    Task AddStockAlertsAsync(List<StockAlertInfo> alerts);
    Task ClearAllAsync();
    Task DeleteAsync(int notificationId);
    Task<List<Notification>> GetAllAsync();
    Task<List<Notification>> GetUnreadAsync();
    Task<int> GetUnreadCountAsync();
    Task MarkAllAsReadAsync();
    Task MarkAsReadAsync(int notificationId);
}

internal class NotificationService(
    IDbContextFactory<StockDbContext> contextFactory,
    ILogger<NotificationService> logger) : INotificationService
{
    public event Action? OnNotificationChanged;

    public async Task<List<Notification>> GetAllAsync()
    {
        using var dbContext = await contextFactory.CreateDbContextAsync();

        return await dbContext.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(10)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Notification>> GetUnreadAsync()
    {
        using var dbContext = await contextFactory.CreateDbContextAsync();

        return await dbContext.Notifications
            .Where(n => !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(10)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        using var dbContext = await contextFactory.CreateDbContextAsync();

        return await dbContext.Notifications
            .CountAsync(n => !n.IsRead);
    }

    public async Task AddNotificationAsync(
        string title,
        string message,
        NotificationType type = NotificationType.Info)
    {
        using var dbContext = await contextFactory.CreateDbContextAsync();

        var notification = new Notification
        {
            Title = title,
            Message = message,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        await dbContext.Notifications.AddAsync(notification);
        await dbContext.SaveChangesAsync();

        NotifyStateChanged();
    }

    public async Task AddStockAlertsAsync(List<StockAlertInfo> alerts)
    {
        using var dbContext = await contextFactory.CreateDbContextAsync();

        try
        {
            var notifications = alerts.Select(alert => new Notification
            {
                Title = alert.IsOutOfStock ? "Out of stock" : "Low stock alert",
                Message = alert.IsOutOfStock
                ? $"{alert.ProductName} is now out of stock ({alert.NewStock} units)"
                : $"{alert.ProductName} is below minimum level ({alert.NewStock}/{alert.MinimumStockLevel} units)",
                Type = alert.IsOutOfStock ? NotificationType.Error : NotificationType.Warning,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            }).ToList();

            await dbContext.Notifications.AddRangeAsync(notifications);
            await dbContext.SaveChangesAsync();

            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while saving notifications");
            throw new InternalServerException();
        }
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        using var dbContext = await contextFactory.CreateDbContextAsync();

        await dbContext.Notifications
            .Where(n => n.Id == notificationId)
            .ExecuteUpdateAsync(n => n.SetProperty(x => x.IsRead, true));

        NotifyStateChanged();
    }

    public async Task MarkAllAsReadAsync()
    {
        using var dbContext = await contextFactory.CreateDbContextAsync();

        await dbContext.Notifications
            .Where(n => !n.IsRead)
            .ExecuteUpdateAsync(n => n.SetProperty(x => x.IsRead, true));

        NotifyStateChanged();
    }

    public async Task DeleteAsync(int notificationId)
    {
        using var dbContext = await contextFactory.CreateDbContextAsync();

        await dbContext.Notifications
            .Where(n => n.Id == notificationId)
            .ExecuteDeleteAsync();

        NotifyStateChanged();
    }

    public async Task ClearAllAsync()
    {
        using var dbContext = await contextFactory.CreateDbContextAsync();

        await dbContext.Notifications.ExecuteDeleteAsync();

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnNotificationChanged?.Invoke();
}
