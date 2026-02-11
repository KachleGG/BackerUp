using BackerUp.Core;
using Cronos;

namespace BackerUp.Client.Services;

public static class CronTimingService {
    public static bool IsJobDue(BackupJob? job, JobsMetadata? jobMeta) {
        if (job == null) {
            return false;
        }

        if (string.IsNullOrWhiteSpace(job.Timing)) {
            return true;
        }

        DateTime nowUtc = DateTime.UtcNow;

        try {
            CronExpression cron = CronExpression.Parse(job.Timing, CronFormat.Standard);

            DateTime? lastRun = jobMeta?.LastSnapshotTimestampUtc;
            DateTime from;
            if (!lastRun.HasValue || lastRun.Value == DateTime.MinValue) {
                // if never run, search from a reasonable past window
                from = nowUtc.AddYears(-1);
            } else {
                from = lastRun.Value.AddSeconds(1);
            }

            DateTime? next = cron.GetNextOccurrence(from, TimeZoneInfo.Utc);
            return next.HasValue && next.Value <= nowUtc;
        } catch {
            return false;
        }
    }

    public static DateTime? GetNextOccurrenceForJob(BackupJob? job, JobsMetadata? jobMeta, DateTime nowUtc) {
        if (job == null || string.IsNullOrWhiteSpace(job.Timing)) {
            return null;
        }

        try {
            CronExpression cron = CronExpression.Parse(job.Timing, CronFormat.Standard);
            DateTime from = (jobMeta?.LastSnapshotTimestampUtc ?? nowUtc).AddSeconds(1);
            return cron.GetNextOccurrence(from, TimeZoneInfo.Utc);
        } catch {
            return null;
        }
    }
}