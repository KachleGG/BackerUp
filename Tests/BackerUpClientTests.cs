using BackerUp_Client.Models;
using BackerUp_Client.Services;
using Class_Lib;

namespace Tests;

[TestClass]
public sealed class BackerUpClientTests
{
    private string _testRoot = "";
    private string _sourceDir = "";
    private string _targetDir = "";

    [TestInitialize]
    public void Setup() {
        _testRoot = Path.Combine(Path.GetTempPath(), "BackerUpTests_" + Guid.NewGuid().ToString());
        _sourceDir = Path.Combine(_testRoot, "source");
        _targetDir = Path.Combine(_testRoot, "target");
        Directory.CreateDirectory(_sourceDir);
        Directory.CreateDirectory(_targetDir);
    }

    [TestCleanup]
    public void Cleanup() {
        try {
            if (Directory.Exists(_testRoot)) {
                Directory.Delete(_testRoot, true);
            }
        } catch { }
    }

    [TestMethod]
    public void FullBackup_CreatesPackageAndUpdatesTimestamps() {
        // Arrange
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        
        BackupJob job = new() {
            Id = 1,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = BackupMethod.Full,
            Timing = "",
            BackupRetention = new BackupRetention { Count = 5, Size = 10 }
        };

        JobsMetadata meta = new() { Job = job };
        Backup backup = new BackupFull();

        DateTime beforeBackup = DateTime.UtcNow.AddSeconds(-1);

        // Act
        backup.PerformBackup(job, meta);

        // Assert
        Assert.IsNotNull(meta.LastPackageTimestampUtc, "LastPackageTimestampUtc should be set");
        Assert.IsNotNull(meta.LastSnapshotTimestampUtc, "LastSnapshotTimestampUtc should be set");
        Assert.IsTrue(meta.LastPackageTimestampUtc >= beforeBackup, "Package timestamp should be recent");
        Assert.IsTrue(meta.LastSnapshotTimestampUtc >= beforeBackup, "Snapshot timestamp should be recent");
        Assert.AreEqual(1, meta.Packages.Count, "Should have one package");
        Assert.AreEqual(1, meta.Packages[0].SnapshotCount, "Full backup should increment snapshot count");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, meta.Packages[0].Name, "fullBackup")), "Full backup directory should exist");
    }

    [TestMethod]
    public void IncrementalBackup_FirstRun_PerformsFullBackup() {
        // Arrange
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        
        BackupJob job = new() {
            Id = 2,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = BackupMethod.Incremental,
            Timing = "",
            BackupRetention = new BackupRetention { Count = 5, Size = 10 }
        };

        JobsMetadata meta = new() { Job = job };
        Backup backup = new BackupIncremental();

        // Act
        backup.PerformBackup(job, meta);

        // Assert
        Assert.AreEqual(1, meta.Packages.Count, "Should have one package");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, meta.Packages[0].Name, "fullBackup")), "Should perform full backup on first run");
        Assert.IsNotNull(meta.LastSnapshotTimestampUtc, "LastSnapshotTimestampUtc should be set");
    }

    [TestMethod]
    public void IncrementalBackup_SubsequentRun_CreatesSnapshot() {
        // Arrange
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        
        BackupJob job = new() {
            Id = 3,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = BackupMethod.Incremental,
            Timing = "",
            BackupRetention = new BackupRetention { Count = 5, Size = 10 }
        };

        JobsMetadata meta = new() { Job = job };
        Backup backup = new BackupIncremental();

        // First backup
        backup.PerformBackup(job, meta);
        Assert.AreEqual(1, meta.Packages[0].SnapshotCount, "First backup should create one snapshot");

        // Simulate time passing and file change
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file2.txt"), "content2");

        // Act - Second backup
        backup.PerformBackup(job, meta);

        // Assert
        Assert.AreEqual(1, meta.Packages.Count, "Should still have one package");
        Assert.AreEqual(2, meta.Packages[0].SnapshotCount, "Should have two snapshots");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, meta.Packages[0].Name, "snapshot_1")), "Second snapshot should exist");
    }

    [TestMethod]
    public void DifferentialBackup_FirstRun_PerformsFullBackup() {
        // Arrange
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        
        BackupJob job = new() {
            Id = 4,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = BackupMethod.Differential,
            Timing = "",
            BackupRetention = new BackupRetention { Count = 5, Size = 10 }
        };

        JobsMetadata meta = new() { Job = job };
        Backup backup = new BackupDifferential();

        // Act
        backup.PerformBackup(job, meta);

        // Assert
        Assert.AreEqual(1, meta.Packages.Count, "Should have one package");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, meta.Packages[0].Name, "fullBackup")), "Should perform full backup on first run");
        Assert.IsNotNull(meta.LastSnapshotTimestampUtc, "LastSnapshotTimestampUtc should be set");
    }

    [TestMethod]
    public void DifferentialBackup_SubsequentRuns_UseLastPackageTimestamp() {
        // Arrange
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        
        BackupJob job = new() {
            Id = 5,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = BackupMethod.Differential,
            Timing = "",
            BackupRetention = new BackupRetention { Count = 5, Size = 10 }
        };

        JobsMetadata meta = new() { Job = job };
        Backup backup = new BackupDifferential();

        // First backup (full)
        backup.PerformBackup(job, meta);
        DateTime firstPackageTime = meta.LastPackageTimestampUtc.Value;

        // Simulate time passing and file changes
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file2.txt"), "content2");

        // Second backup (differential snapshot 1 - should include file1 + file2 changed since package start)
        backup.PerformBackup(job, meta);
        Assert.AreEqual(2, meta.Packages[0].SnapshotCount, "Should have two snapshots");

        // Simulate more time passing and more file changes
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file3.txt"), "content3");

        // Act - Third backup (differential snapshot 2 - should include file1/2/3 all changed since package start)
        backup.PerformBackup(job, meta);

        // Assert
        Assert.AreEqual(1, meta.Packages.Count, "Should still have one package");
        Assert.AreEqual(3, meta.Packages[0].SnapshotCount, "Should have three snapshots");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, meta.Packages[0].Name, "snapshot_1")), "Snapshot 1 should exist");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, meta.Packages[0].Name, "snapshot_2")), "Snapshot 2 should exist");
        Assert.AreEqual(firstPackageTime, meta.LastPackageTimestampUtc, "Package timestamp should not change until new package created");
    }

    [TestMethod]
    public void IncrementalBackup_ReachesSnapshotLimit_CreatesNewPackage() {
        // Arrange
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        
        BackupJob job = new() {
            Id = 6,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = BackupMethod.Incremental,
            Timing = "",
            BackupRetention = new BackupRetention { Count = 5, Size = 2 } // Only 2 snapshots per package
        };

        JobsMetadata meta = new() { Job = job };
        Backup backup = new BackupIncremental();

        // First backup (full)
        backup.PerformBackup(job, meta);
        string firstPackageName = meta.Packages[0].Name;

        // Second backup (snapshot 1)
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file2.txt"), "content2");
        backup.PerformBackup(job, meta);

        // Act - Third backup (should create new package because we've reached the limit of 2)
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file3.txt"), "content3");
        backup.PerformBackup(job, meta);

        // Assert
        Assert.AreEqual(2, meta.Packages.Count, "Should have two packages");
        Assert.AreEqual(2, meta.Packages[0].SnapshotCount, "First package should have 2 snapshots");
        Assert.AreEqual(1, meta.Packages[1].SnapshotCount, "Second package should have 1 snapshot (new full backup)");
        Assert.AreNotEqual(firstPackageName, meta.Packages[1].Name, "Second package should have different name");
    }

    [TestMethod]
    public void DifferentialBackup_ReachesSnapshotLimit_CreatesNewPackage() {
        // Arrange
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        
        BackupJob job = new() {
            Id = 7,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = BackupMethod.Differential,
            Timing = "",
            BackupRetention = new BackupRetention { Count = 5, Size = 3 } // Only 3 snapshots per package
        };

        JobsMetadata meta = new() { Job = job };
        Backup backup = new BackupDifferential();

        // First backup (full)
        backup.PerformBackup(job, meta);
        string firstPackageName = meta.Packages[0].Name;

        // Second backup (differential snapshot 1)
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file2.txt"), "content2");
        backup.PerformBackup(job, meta);

        // Third backup (differential snapshot 2)
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file3.txt"), "content3");
        backup.PerformBackup(job, meta);

        // Act - Fourth backup (should create new package because we've reached the limit of 3)
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file4.txt"), "content4");
        backup.PerformBackup(job, meta);

        // Assert
        Assert.AreEqual(2, meta.Packages.Count, "Should have two packages");
        Assert.AreEqual(3, meta.Packages[0].SnapshotCount, "First package should have 3 snapshots");
        Assert.AreEqual(1, meta.Packages[1].SnapshotCount, "Second package should have 1 snapshot (new full backup)");
        Assert.AreNotEqual(firstPackageName, meta.Packages[1].Name, "Second package should have different name");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, meta.Packages[1].Name, "fullBackup")), "New package should have full backup");
    }

    [TestMethod]
    public void IncrementalBackup_CreateMultiplePackages_WithRetention() {
        // Arrange
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        
        BackupJob job = new() {
            Id = 8,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = BackupMethod.Incremental,
            Timing = "",
            BackupRetention = new BackupRetention { Count = 2, Size = 2 } // Keep only 2 packages, 2 snapshots each
        };

        JobsMetadata meta = new() { Job = job };
        Backup backup = new BackupIncremental();

        List<string> packageNames = new();

        // Create first package with 2 snapshots
        backup.PerformBackup(job, meta);
        packageNames.Add(meta.Packages[0].Name);
        
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file2.txt"), "content2");
        backup.PerformBackup(job, meta);

        // Create second package with 2 snapshots
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file3.txt"), "content3");
        backup.PerformBackup(job, meta);
        packageNames.Add(meta.Packages[1].Name);
        
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file4.txt"), "content4");
        backup.PerformBackup(job, meta);

        // Act - Create third package (should trigger retention and delete first package)
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file5.txt"), "content5");
        backup.PerformBackup(job, meta);
        packageNames.Add(meta.Packages[1].Name); // After retention, index shifts

        // Assert
        Assert.AreEqual(2, meta.Packages.Count, "Should only keep 2 packages due to retention policy");
        Assert.IsFalse(Directory.Exists(Path.Combine(_targetDir, packageNames[0])), "First package should be deleted");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, packageNames[1])), "Second package should exist");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, packageNames[2])), "Third package should exist");
    }

    [TestMethod]
    public void DifferentialBackup_CreateMultiplePackages_AllSnapshotsRelativeToPackageStart() {
        // Arrange
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        
        BackupJob job = new() {
            Id = 9,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = BackupMethod.Differential,
            Timing = "",
            BackupRetention = new BackupRetention { Count = 5, Size = 2 } // 2 snapshots per package
        };

        JobsMetadata meta = new() { Job = job };
        Backup backup = new BackupDifferential();

        // Package 1: Full backup
        backup.PerformBackup(job, meta);
        string pkg1Name = meta.Packages[0].Name;
        
        // Package 1: Differential snapshot (should have file1 + file2)
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file2.txt"), "content2");
        backup.PerformBackup(job, meta);

        // Package 2: Full backup (new package triggered)
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file3.txt"), "content3");
        backup.PerformBackup(job, meta);
        string pkg2Name = meta.Packages[1].Name;

        // Act - Package 2: Differential snapshot (should compare to package 2 start, include file3 + file4)
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file4.txt"), "content4");
        backup.PerformBackup(job, meta);

        // Assert
        Assert.AreEqual(2, meta.Packages.Count, "Should have two packages");
        Assert.AreEqual(2, meta.Packages[0].SnapshotCount, "First package should have 2 snapshots");
        Assert.AreEqual(2, meta.Packages[1].SnapshotCount, "Second package should have 2 snapshots");
        
        // Verify directory structure
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, pkg1Name, "fullBackup")), "Package 1 full backup exists");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, pkg1Name, "snapshot_1")), "Package 1 differential snapshot exists");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, pkg2Name, "fullBackup")), "Package 2 full backup exists");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, pkg2Name, "snapshot_1")), "Package 2 differential snapshot exists");
    }

    [TestMethod]
    public void FullBackup_MultipleRuns_CreatesMultiplePackages() {
        // Arrange
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        
        BackupJob job = new() {
            Id = 10,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = BackupMethod.Full,
            Timing = "",
            BackupRetention = new BackupRetention { Count = 3, Size = 1 } // Keep 3 packages, 1 snapshot each (full)
        };

        JobsMetadata meta = new() { Job = job };
        Backup backup = new BackupFull();

        List<string> packageNames = new();

        // Act - Create 4 full backups
        for (int i = 0; i < 4; i++) {
            Thread.Sleep(100);
            File.WriteAllText(Path.Combine(_sourceDir, $"file{i + 2}.txt"), $"content{i + 2}");
            backup.PerformBackup(job, meta);
            if (meta.Packages.Count > 0) {
                packageNames.Add(meta.GetCurrentPackage()!.Name);
            }
        }

        // Assert
        Assert.AreEqual(3, meta.Packages.Count, "Should only keep 3 packages due to retention policy");
        Assert.IsFalse(Directory.Exists(Path.Combine(_targetDir, packageNames[0])), "First package should be deleted by retention");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, packageNames[1])), "Second package should exist");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, packageNames[2])), "Third package should exist");
        Assert.IsTrue(Directory.Exists(Path.Combine(_targetDir, packageNames[3])), "Fourth package should exist");
    }

    [TestMethod]
    public void IncrementalBackup_NoChanges_DoesNotCreateSnapshot() {
        // Arrange
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        
        BackupJob job = new() {
            Id = 11,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = BackupMethod.Incremental,
            Timing = "",
            BackupRetention = new BackupRetention { Count = 5, Size = 10 }
        };

        JobsMetadata meta = new() { Job = job };
        Backup backup = new BackupIncremental();

        // First backup
        backup.PerformBackup(job, meta);
        Assert.AreEqual(1, meta.Packages[0].SnapshotCount, "First backup should create one snapshot");

        Thread.Sleep(100);
        
        // Act - Second backup with no file changes
        backup.PerformBackup(job, meta);

        // Assert
        Assert.AreEqual(1, meta.Packages[0].SnapshotCount, "Should not create new snapshot when no files changed");
    }

    [TestMethod]
    public void DifferentialBackup_NoChanges_DoesNotCreateSnapshot() {
        // Arrange
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "content1");
        
        BackupJob job = new() {
            Id = 12,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = BackupMethod.Differential,
            Timing = "",
            BackupRetention = new BackupRetention { Count = 5, Size = 10 }
        };

        JobsMetadata meta = new() { Job = job };
        Backup backup = new BackupDifferential();

        // First backup
        backup.PerformBackup(job, meta);
        Assert.AreEqual(1, meta.Packages[0].SnapshotCount, "First backup should create one snapshot");

        Thread.Sleep(100);
        
        // Act - Second backup with no file changes
        backup.PerformBackup(job, meta);

        // Assert
        Assert.AreEqual(1, meta.Packages[0].SnapshotCount, "Should not create new snapshot when no files changed");
    }

    [TestMethod]
    public void CronTimingService_IsJobDue_UsesLastSnapshotTimestamp() {
        // Arrange
        BackupJob job = new() {
            Id = 13,
            Timing = "*/5 * * * *", // Every 5 minutes
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir }
        };

        JobsMetadata meta = new() { 
            Job = job,
            LastSnapshotTimestampUtc = DateTime.UtcNow.AddMinutes(-6) // 6 minutes ago
        };

        // Act
        bool isDue = CronTimingService.IsJobDue(job, meta);

        // Assert
        Assert.IsTrue(isDue, "Job should be due because last snapshot was 6 minutes ago and cron is every 5 minutes");
    }

    [TestMethod]
    public void CronTimingService_GetNextOccurrence_UsesLastSnapshotTimestamp() {
        // Arrange
        BackupJob job = new() {
            Id = 14,
            Timing = "*/30 * * * *", // Every 30 minutes (easier to test)
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir }
        };

        DateTime lastSnapshot = DateTime.UtcNow.AddMinutes(-15); // 15 minutes ago
        JobsMetadata meta = new() { 
            Job = job,
            LastSnapshotTimestampUtc = lastSnapshot,
            LastPackageTimestampUtc = DateTime.UtcNow.AddDays(-5) // Package is old, but snapshot is recent
        };

        // Act
        DateTime? next = CronTimingService.GetNextOccurrenceForJob(job, meta, DateTime.UtcNow);

        // Assert
        Assert.IsNotNull(next, "Next occurrence should be calculated");
        Assert.IsTrue(next > lastSnapshot, "Next occurrence should be after last snapshot");
        Assert.IsTrue(next > DateTime.UtcNow, "Next occurrence should be in the future");
        
        // Should be approximately 15 minutes from now (30min cron - 15min since last)
        TimeSpan timeUntilNext = next.Value - DateTime.UtcNow;
        Assert.IsTrue(timeUntilNext.TotalMinutes >= 10 && timeUntilNext.TotalMinutes <= 20, 
            $"Next occurrence should be in ~15 minutes, but was {timeUntilNext.TotalMinutes:F2} minutes");
    }

    [TestMethod]
    public void CronTimingService_GetNextOccurrence_WithNoLastSnapshot_UsesNow() {
        // Arrange
        BackupJob job = new() {
            Id = 15,
            Timing = "*/15 * * * *", // Every 15 minutes
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir }
        };

        JobsMetadata meta = new() { 
            Job = job,
            LastSnapshotTimestampUtc = null,
            LastPackageTimestampUtc = null
        };

        DateTime now = DateTime.UtcNow;

        // Act
        DateTime? next = CronTimingService.GetNextOccurrenceForJob(job, meta, now);

        // Assert
        Assert.IsNotNull(next, "Next occurrence should be calculated");
        Assert.IsTrue(next >= now, "Next occurrence should be now or in the future");
        
        // Should be within the next 15 minutes
        TimeSpan timeUntilNext = next.Value - now;
        Assert.IsTrue(timeUntilNext.TotalMinutes <= 16, 
            $"Next occurrence should be within 15 minutes, but was {timeUntilNext.TotalMinutes:F2} minutes");
    }

    private class BackupFull : Backup { }
}
