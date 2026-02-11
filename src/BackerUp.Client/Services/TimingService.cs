using BackerUp.Core;
using Quartz;

namespace BackerUp.Client.Services;

public static class TimingService {
    public static bool IsJobDue(BackupJob? job, JobsMetadata? jobMeta) {
        if (job == null) {
            return false;
        }

        if (string.IsNullOrWhiteSpace(job.Timing)) {
            return true;
        }

        DateTime nowUtc = DateTime.UtcNow;

        try {
            CronExpression cron = new CronExpression(job.Timing);
            cron.TimeZone = TimeZoneInfo.Utc;

            DateTime? lastRun = jobMeta?.LastSnapshotTimestampUtc;
            DateTimeOffset from;
            if (!lastRun.HasValue || lastRun.Value == DateTime.MinValue) {
                // if never run, search from a reasonable past window
                from = new DateTimeOffset(nowUtc.AddYears(-1), TimeSpan.Zero);
            } else {
                from = new DateTimeOffset(lastRun.Value.AddSeconds(1), TimeSpan.Zero);
            }

            DateTimeOffset? next = cron.GetNextValidTimeAfter(from);
            return next.HasValue && next.Value.UtcDateTime <= nowUtc;
        } catch {
            return false;
        }
    }

    public static DateTime? GetNextOccurrenceForJob(BackupJob? job, JobsMetadata? jobMeta, DateTime nowUtc) {
        if (job == null || string.IsNullOrWhiteSpace(job.Timing)) {
            return null;
        }

        try {
            CronExpression cron = new CronExpression(job.Timing);
            cron.TimeZone = TimeZoneInfo.Utc;
            DateTimeOffset from = new DateTimeOffset((jobMeta?.LastSnapshotTimestampUtc ?? nowUtc).AddSeconds(1), TimeSpan.Zero);
            DateTimeOffset? next = cron.GetNextValidTimeAfter(from);
            return next?.UtcDateTime;
        } catch {
            return null;
        }
    }
}