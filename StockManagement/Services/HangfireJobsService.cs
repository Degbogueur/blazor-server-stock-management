using Hangfire;

namespace StockManagement.Services;

public static class HangfireJobsService
{
    public static void ConfigureRecurringJobs(CancellationToken token = default)
    {
        RecurringJob.AddOrUpdate<IStockSnapshotService>(
            "weekly-stock-snapshot",
            service => service.TakeWeeklySnapshotsAsync(token),
            Cron.Weekly(DayOfWeek.Monday, 10, 0)
        );
    }
}
