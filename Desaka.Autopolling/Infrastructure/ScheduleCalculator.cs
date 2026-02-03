namespace Desaka.Autopolling.Infrastructure;

/// <summary>
/// Calculates next run times for scheduled operations.
/// </summary>
public interface IScheduleCalculator
{
    DateTime CalculateNextRun(ScheduleConfig config, DateTime lastRun);
    bool ShouldRunNow(ScheduleConfig config, DateTime lastRun);
}

public record ScheduleConfig(
    TimeSpan Interval,
    TimeOnly? PreferredTime = null,
    DayOfWeek[]? PreferredDays = null,
    bool Enabled = true
);

public sealed class ScheduleCalculator : IScheduleCalculator
{
    public DateTime CalculateNextRun(ScheduleConfig config, DateTime lastRun)
    {
        if (!config.Enabled)
            return DateTime.MaxValue;

        var nextRun = lastRun.Add(config.Interval);

        // If preferred time is set, adjust to that time
        if (config.PreferredTime.HasValue)
        {
            var preferred = config.PreferredTime.Value;
            nextRun = new DateTime(nextRun.Year, nextRun.Month, nextRun.Day,
                preferred.Hour, preferred.Minute, preferred.Second, DateTimeKind.Utc);

            // If we've passed the preferred time today, move to next day
            if (nextRun <= lastRun)
                nextRun = nextRun.AddDays(1);
        }

        // If preferred days are set, find next valid day
        if (config.PreferredDays != null && config.PreferredDays.Length > 0)
        {
            while (!config.PreferredDays.Contains(nextRun.DayOfWeek))
            {
                nextRun = nextRun.AddDays(1);
            }
        }

        return nextRun;
    }

    public bool ShouldRunNow(ScheduleConfig config, DateTime lastRun)
    {
        if (!config.Enabled)
            return false;

        var nextRun = CalculateNextRun(config, lastRun);
        return DateTime.UtcNow >= nextRun;
    }
}
