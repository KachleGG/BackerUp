using BackerUp.Client.Services;
using BackerUp.Core;

namespace BackerUp.Tests.Client;

[TestClass]
[DoNotParallelize]
public class BackupServiceTests
{
    [TestMethod]
    public void Constructor_WithValidJobs_SetsBackupJobs()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 1 },
            new BackupJob { Id = 2 }
        };

        var service = new BackupService(jobs);

        Assert.IsNotNull(service.BackupJobs);
        Assert.AreEqual(2, service.BackupJobs.Count);
    }

    [TestMethod]
    public void Constructor_WithEmptyList_SetsEmptyBackupJobs()
    {
        var jobs = new List<BackupJob>();

        var service = new BackupService(jobs);

        Assert.IsNotNull(service.BackupJobs);
        Assert.AreEqual(0, service.BackupJobs.Count);
    }

    [TestMethod]
    public void Constructor_WithNullList_SetsNullBackupJobs()
    {
        var service = new BackupService(null!);

        Assert.IsNull(service.BackupJobs);
    }

    [TestMethod]
    public async Task RunAsync_WithNullBackupJobs_ReturnsImmediately()
    {
        var service = new BackupService(null!);

        // Should not throw and return quickly
        await service.RunAsync();
    }

    [TestMethod]
    public async Task RunAsync_WithEmptyBackupJobs_ReturnsImmediately()
    {
        var service = new BackupService(new List<BackupJob>());

        // Should not throw and return quickly
        await service.RunAsync();
    }

    [TestMethod]
    public async Task RunAsync_WithCancellationToken_RespectsCancellation()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 401, Timing = "0 0 0 1 1 ? 2099", Sources = new List<string>(), Targets = new List<string>() }
        };
        var service = new BackupService(jobs);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Should complete without throwing OperationCanceledException to caller
        await service.RunAsync(cts.Token);
    }

    [TestMethod]
    public void BackupJobs_CanBeModified()
    {
        var jobs = new List<BackupJob> { new BackupJob { Id = 1 } };
        var service = new BackupService(jobs);

        service.BackupJobs.Add(new BackupJob { Id = 2 });

        Assert.AreEqual(2, service.BackupJobs.Count);
    }

    [TestMethod]
    public void BackupJobs_CanBeReplaced()
    {
        var jobs = new List<BackupJob> { new BackupJob { Id = 1 } };
        var service = new BackupService(jobs);

        service.BackupJobs = new List<BackupJob>
        {
            new BackupJob { Id = 3 },
            new BackupJob { Id = 4 },
            new BackupJob { Id = 5 }
        };

        Assert.AreEqual(3, service.BackupJobs.Count);
        Assert.AreEqual(3, service.BackupJobs[0].Id);
    }

    [TestMethod]
    public async Task RunAsync_WithMultipleJobs_ProcessesAll()
    {
        // Note: In DEBUG mode, timing check is skipped, so jobs will try to run.
        // Using empty Sources/Targets to prevent actual file operations.
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 201, Timing = "0 0 0 1 1 ? 2099", Sources = new List<string>(), Targets = new List<string>() },
            new BackupJob { Id = 202, Timing = "0 0 0 1 1 ? 2099", Sources = new List<string>(), Targets = new List<string>() },
            new BackupJob { Id = 203, Timing = "0 0 0 1 1 ? 2099", Sources = new List<string>(), Targets = new List<string>() }
        };
        var service = new BackupService(jobs);

        // Should complete without throwing
        await service.RunAsync();
    }

    [TestMethod]
    public async Task RunAsync_WithNullJobInList_SkipsNullJob()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 301, Timing = "0 0 0 1 1 ? 2099", Sources = new List<string>(), Targets = new List<string>() },
            null!,
            new BackupJob { Id = 303, Timing = "0 0 0 1 1 ? 2099", Sources = new List<string>(), Targets = new List<string>() }
        };
        var service = new BackupService(jobs);

        // Should not throw despite null in list
        await service.RunAsync();
    }

    [TestMethod]
    public async Task RunAsync_CalledMultipleTimes_WorksCorrectly()
    {
        // Use a far-future cron that will never trigger to avoid file locking issues
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 999, Timing = "0 0 0 1 1 ? 2099", Sources = new List<string>(), Targets = new List<string>() }
        };
        var service = new BackupService(jobs);

        // These should complete quickly since jobs aren't due
        await service.RunAsync();
        await service.RunAsync();
        await service.RunAsync();

        // Should complete all calls without issues
    }

    [TestMethod]
    public void Constructor_PreservesJobOrder()
    {
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 5 },
            new BackupJob { Id = 3 },
            new BackupJob { Id = 1 },
            new BackupJob { Id = 4 },
            new BackupJob { Id = 2 }
        };

        var service = new BackupService(jobs);

        Assert.AreEqual(5, service.BackupJobs[0].Id);
        Assert.AreEqual(3, service.BackupJobs[1].Id);
        Assert.AreEqual(1, service.BackupJobs[2].Id);
        Assert.AreEqual(4, service.BackupJobs[3].Id);
        Assert.AreEqual(2, service.BackupJobs[4].Id);
    }

    [TestMethod]
    public async Task RunAsync_WithDifferentBackupMethods_HandlesAll()
    {
        // Use far-future cron times to avoid actual execution and file locking
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = 101, Method = BackupMethod.Full, Timing = "0 0 0 1 1 ? 2099", Sources = new List<string>(), Targets = new List<string>() },
            new BackupJob { Id = 102, Method = BackupMethod.Differential, Timing = "0 0 0 1 1 ? 2099", Sources = new List<string>(), Targets = new List<string>() },
            new BackupJob { Id = 103, Method = BackupMethod.Incremental, Timing = "0 0 0 1 1 ? 2099", Sources = new List<string>(), Targets = new List<string>() }
        };
        var service = new BackupService(jobs);

        await service.RunAsync();

        // Should handle all backup methods without error
    }
}

