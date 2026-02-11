using BackerUp.Client.Services;
using BackerUp.Core;

namespace BackerUp.Tests.Client;

[TestClass]
public class TimingServiceTests
{
    #region IsJobDue Tests

    [TestMethod]
    public void IsJobDue_NullJob_ReturnsFalse()
    {
        bool result = TimingService.IsJobDue(null, null);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsJobDue_EmptyTiming_ReturnsTrue()
    {
        var job = new BackupJob { Id = 1, Timing = "" };
        bool result = TimingService.IsJobDue(job, null);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsJobDue_WhitespaceTiming_ReturnsTrue()
    {
        var job = new BackupJob { Id = 1, Timing = "   " };
        bool result = TimingService.IsJobDue(job, null);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsJobDue_NullTiming_ReturnsTrue()
    {
        var job = new BackupJob { Id = 1, Timing = null! };
        bool result = TimingService.IsJobDue(job, null);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsJobDue_InvalidCronExpression_ReturnsFalse()
    {
        var job = new BackupJob { Id = 1, Timing = "invalid cron" };
        bool result = TimingService.IsJobDue(job, null);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsJobDue_NeverRun_EveryMinuteCron_ReturnsTrue()
    {
        // Every minute cron - should always be due if never run
        var job = new BackupJob { Id = 1, Timing = "0 * * * * ?" };
        var jobMeta = new JobsMetadata { LastSnapshotTimestampUtc = null };

        bool result = TimingService.IsJobDue(job, jobMeta);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsJobDue_RecentlyRun_EveryMinuteCron_ReturnsFalse()
    {
        // Every minute cron - should not be due if just run
        var job = new BackupJob { Id = 1, Timing = "0 * * * * ?" };
        var jobMeta = new JobsMetadata { LastSnapshotTimestampUtc = DateTime.UtcNow };

        bool result = TimingService.IsJobDue(job, jobMeta);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsJobDue_PastDue_ReturnsTrue()
    {
        // Every minute cron, last run was 2 minutes ago
        var job = new BackupJob { Id = 1, Timing = "0 * * * * ?" };
        var jobMeta = new JobsMetadata { LastSnapshotTimestampUtc = DateTime.UtcNow.AddMinutes(-2) };

        bool result = TimingService.IsJobDue(job, jobMeta);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsJobDue_LastRunMinValue_TreatedAsNeverRun()
    {
        var job = new BackupJob { Id = 1, Timing = "0 * * * * ?" };
        var jobMeta = new JobsMetadata { LastSnapshotTimestampUtc = DateTime.MinValue };

        bool result = TimingService.IsJobDue(job, jobMeta);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsJobDue_NullJobMeta_WithValidCron_ReturnsTrue()
    {
        var job = new BackupJob { Id = 1, Timing = "0 * * * * ?" };

        bool result = TimingService.IsJobDue(job, null);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsJobDue_DailyCron_NotDueYet_ReturnsFalse()
    {
        // Daily at midnight - if we just ran, shouldn't be due
        var job = new BackupJob { Id = 1, Timing = "0 0 0 * * ?" };
        var jobMeta = new JobsMetadata { LastSnapshotTimestampUtc = DateTime.UtcNow.AddHours(-1) };

        bool result = TimingService.IsJobDue(job, jobMeta);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsJobDue_YearlyCron_NeverRun_ReturnsTrue()
    {
        // January 1st at midnight - should be due if never run (checks past year)
        var job = new BackupJob { Id = 1, Timing = "0 0 0 1 1 ?" };
        var jobMeta = new JobsMetadata { LastSnapshotTimestampUtc = null };

        bool result = TimingService.IsJobDue(job, jobMeta);
        Assert.IsTrue(result);
    }

    #endregion

    #region GetNextOccurrenceForJob Tests

    [TestMethod]
    public void GetNextOccurrenceForJob_NullJob_ReturnsNull()
    {
        DateTime? result = TimingService.GetNextOccurrenceForJob(null, null, DateTime.UtcNow);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetNextOccurrenceForJob_EmptyTiming_ReturnsNull()
    {
        var job = new BackupJob { Id = 1, Timing = "" };
        DateTime? result = TimingService.GetNextOccurrenceForJob(job, null, DateTime.UtcNow);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetNextOccurrenceForJob_NullTiming_ReturnsNull()
    {
        var job = new BackupJob { Id = 1, Timing = null! };
        DateTime? result = TimingService.GetNextOccurrenceForJob(job, null, DateTime.UtcNow);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetNextOccurrenceForJob_InvalidCron_ReturnsNull()
    {
        var job = new BackupJob { Id = 1, Timing = "not a cron" };
        DateTime? result = TimingService.GetNextOccurrenceForJob(job, null, DateTime.UtcNow);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetNextOccurrenceForJob_ValidCron_ReturnsNextTime()
    {
        var job = new BackupJob { Id = 1, Timing = "0 * * * * ?" };
        DateTime now = DateTime.UtcNow;

        DateTime? result = TimingService.GetNextOccurrenceForJob(job, null, now);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Value > now);
    }

    [TestMethod]
    public void GetNextOccurrenceForJob_WithLastRun_ReturnsNextAfterLastRun()
    {
        var job = new BackupJob { Id = 1, Timing = "0 * * * * ?" };
        DateTime lastRun = DateTime.UtcNow.AddMinutes(-5);
        var jobMeta = new JobsMetadata { LastSnapshotTimestampUtc = lastRun };

        DateTime? result = TimingService.GetNextOccurrenceForJob(job, jobMeta, DateTime.UtcNow);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Value > lastRun);
    }

    [TestMethod]
    public void GetNextOccurrenceForJob_EveryHourCron_ReturnsCorrectNext()
    {
        var job = new BackupJob { Id = 1, Timing = "0 0 * * * ?" };
        DateTime now = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        DateTime? result = TimingService.GetNextOccurrenceForJob(job, null, now);

        Assert.IsNotNull(result);
        Assert.AreEqual(11, result.Value.Hour);
        Assert.AreEqual(0, result.Value.Minute);
    }

    #endregion

    #region Cron Expression Edge Cases

    [TestMethod]
    public void IsJobDue_SpecificDayOfWeekCron_WorksCorrectly()
    {
        // Every Monday at noon
        var job = new BackupJob { Id = 1, Timing = "0 0 12 ? * MON" };
        var jobMeta = new JobsMetadata { LastSnapshotTimestampUtc = null };

        // Should return true (past Mondays exist in the year lookback window)
        bool result = TimingService.IsJobDue(job, jobMeta);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsJobDue_LastDayOfMonthCron_WorksCorrectly()
    {
        // Last day of every month at midnight
        var job = new BackupJob { Id = 1, Timing = "0 0 0 L * ?" };
        var jobMeta = new JobsMetadata { LastSnapshotTimestampUtc = null };

        bool result = TimingService.IsJobDue(job, jobMeta);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsJobDue_EverySecondCron_AlwaysDue()
    {
        // Every second
        var job = new BackupJob { Id = 1, Timing = "* * * * * ?" };
        var jobMeta = new JobsMetadata { LastSnapshotTimestampUtc = DateTime.UtcNow.AddSeconds(-2) };

        bool result = TimingService.IsJobDue(job, jobMeta);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsJobDue_WeekdaysCron_WorksCorrectly()
    {
        // Every weekday at 9 AM
        var job = new BackupJob { Id = 1, Timing = "0 0 9 ? * MON-FRI" };
        var jobMeta = new JobsMetadata { LastSnapshotTimestampUtc = null };

        bool result = TimingService.IsJobDue(job, jobMeta);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetNextOccurrenceForJob_QuarterlySchedule_ReturnsValidDate()
    {
        // First day of Jan, Apr, Jul, Oct at midnight
        var job = new BackupJob { Id = 1, Timing = "0 0 0 1 1,4,7,10 ?" };
        DateTime now = DateTime.UtcNow;

        DateTime? result = TimingService.GetNextOccurrenceForJob(job, null, now);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Value > now);
        Assert.AreEqual(1, result.Value.Day);
        Assert.IsTrue(new[] { 1, 4, 7, 10 }.Contains(result.Value.Month));
    }

    #endregion
}

