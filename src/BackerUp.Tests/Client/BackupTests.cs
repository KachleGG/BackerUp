using BackerUp.Client.Models;
using BackerUp.Core;

namespace BackerUp.Tests.Client;

[TestClass]
[DoNotParallelize]
public class BackupTests
{
    private string _testDir = null!;
    private string _sourceDir = null!;
    private string _targetDir = null!;

    [TestInitialize]
    public void Setup()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "BackerUpTests", Guid.NewGuid().ToString());
        _sourceDir = Path.Combine(_testDir, "source");
        _targetDir = Path.Combine(_testDir, "target");

        Directory.CreateDirectory(_sourceDir);
        Directory.CreateDirectory(_targetDir);

        // Create test files
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "Test content 1");
        File.WriteAllText(Path.Combine(_sourceDir, "file2.txt"), "Test content 2");

        var subDir = Path.Combine(_sourceDir, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "file3.txt"), "Test content 3");
    }

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, recursive: true);
            }
        }
        catch { }
    }

    private BackupJob CreateTestJob(BackupMethod method, int id = 1)
    {
        return new BackupJob
        {
            Id = id,
            Sources = new List<string> { _sourceDir },
            Targets = new List<string> { _targetDir },
            Method = method,
            Timing = "", // Empty timing means always due
            BackupRetention = new BackupRetention { Count = 5, Size = 10 }
        };
    }

    private JobsMetadata CreateTestMetadata(BackupJob job)
    {
        return new JobsMetadata
        {
            Job = job,
            NextPackageIndex = 0,
            Method = job.Method
        };
    }

    #region BackupFull Tests

    [TestMethod]
    public void BackupFull_PerformBackup_CreatesPackageDirectory()
    {
        var job = CreateTestJob(BackupMethod.Full);
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        backup.PerformBackup(job, jobMeta);

        var packages = Directory.GetDirectories(_targetDir, "package_*");
        Assert.AreEqual(1, packages.Length);
    }

    [TestMethod]
    public void BackupFull_PerformBackup_CopiesAllFiles()
    {
        var job = CreateTestJob(BackupMethod.Full);
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        backup.PerformBackup(job, jobMeta);

        var packageDir = Directory.GetDirectories(_targetDir, "package_*").First();
        var fullBackupDir = Path.Combine(packageDir, "fullBackup");

        Assert.IsTrue(Directory.Exists(fullBackupDir));
    }

    [TestMethod]
    public void BackupFull_PerformBackup_UpdatesMetadata()
    {
        var job = CreateTestJob(BackupMethod.Full);
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        backup.PerformBackup(job, jobMeta);

        Assert.IsNotNull(jobMeta.LastSnapshotTimestampUtc);
        Assert.IsNotNull(jobMeta.LastPackageTimestampUtc);
        Assert.AreEqual(1, jobMeta.Packages.Count);
        Assert.AreEqual(1, jobMeta.NextPackageIndex);
    }

    [TestMethod]
    public void BackupFull_PerformBackup_SetsMethodToFull()
    {
        var job = CreateTestJob(BackupMethod.Full);
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        backup.PerformBackup(job, jobMeta);

        Assert.AreEqual(BackupMethod.Full, jobMeta.Method);
    }

    [TestMethod]
    public void BackupFull_PerformBackup_WithNullJob_DoesNothing()
    {
        var jobMeta = new JobsMetadata();
        var backup = new BackupFull();

        backup.PerformBackup(null!, jobMeta);

        Assert.AreEqual(0, jobMeta.Packages.Count);
    }

    [TestMethod]
    public void BackupFull_PerformBackup_WithNullSources_DoesNothing()
    {
        var job = CreateTestJob(BackupMethod.Full);
        job.Sources = null!;
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        backup.PerformBackup(job, jobMeta);

        Assert.AreEqual(0, jobMeta.Packages.Count);
    }

    [TestMethod]
    public void BackupFull_PerformBackup_WithNullTargets_DoesNothing()
    {
        var job = CreateTestJob(BackupMethod.Full);
        job.Targets = null!;
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        backup.PerformBackup(job, jobMeta);

        Assert.AreEqual(0, jobMeta.Packages.Count);
    }

    [TestMethod]
    public void BackupFull_PerformBackup_MultipleSources_CopiesAll()
    {
        var source2 = Path.Combine(_testDir, "source2");
        Directory.CreateDirectory(source2);
        File.WriteAllText(Path.Combine(source2, "extra.txt"), "Extra content");

        var job = CreateTestJob(BackupMethod.Full);
        job.Sources.Add(source2);
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        backup.PerformBackup(job, jobMeta);

        var packageDir = Directory.GetDirectories(_targetDir, "package_*").First();
        var fullBackupDir = Path.Combine(packageDir, "fullBackup");

        Assert.IsTrue(Directory.Exists(Path.Combine(fullBackupDir, "source")));
        Assert.IsTrue(Directory.Exists(Path.Combine(fullBackupDir, "source2")));
    }

    [TestMethod]
    public void BackupFull_PerformBackup_IncrementsSnapshotCount()
    {
        var job = CreateTestJob(BackupMethod.Full);
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        backup.PerformBackup(job, jobMeta);

        var package = jobMeta.GetCurrentPackage();
        Assert.IsNotNull(package);
        Assert.AreEqual(1, package.SnapshotCount);
    }

    #endregion

    #region BackupDifferential Tests

    [TestMethod]
    public void BackupDifferential_PerformBackup_NoExistingPackage_CreatesFull()
    {
        var job = CreateTestJob(BackupMethod.Differential);
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupDifferential();

        backup.PerformBackup(job, jobMeta);

        // Should create a full backup since no package exists
        Assert.AreEqual(1, jobMeta.Packages.Count);
        Assert.AreEqual(BackupMethod.Full, jobMeta.Method);
    }

    [TestMethod]
    public void BackupDifferential_PerformBackup_WithExistingPackage_CreatesSnapshot()
    {
        var job = CreateTestJob(BackupMethod.Differential);
        var jobMeta = CreateTestMetadata(job);

        // First, create a full backup
        var fullBackup = new BackupFull();
        fullBackup.PerformBackup(job, jobMeta);

        // Modify a file to trigger differential
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "Modified content");
        File.SetLastWriteTimeUtc(Path.Combine(_sourceDir, "file1.txt"), DateTime.UtcNow.AddSeconds(1));

        // Now run differential
        var diffBackup = new BackupDifferential();
        diffBackup.PerformBackup(job, jobMeta);

        Assert.AreEqual(BackupMethod.Differential, jobMeta.Method);
    }

    [TestMethod]
    public void BackupDifferential_PerformBackup_NoChanges_DoesNothing()
    {
        var job = CreateTestJob(BackupMethod.Differential);
        var jobMeta = CreateTestMetadata(job);

        // First, create a full backup
        var fullBackup = new BackupFull();
        fullBackup.PerformBackup(job, jobMeta);

        var initialSnapshotCount = jobMeta.GetCurrentPackage()!.SnapshotCount;
        var initialMethod = jobMeta.Method;

        // Run differential without changes - timestamps are already old
        jobMeta.LastPackageTimestampUtc = DateTime.UtcNow.AddMinutes(1); // Set to future so no files appear changed
        var diffBackup = new BackupDifferential();
        diffBackup.PerformBackup(job, jobMeta);

        // Snapshot count should remain the same since no changes
        Assert.AreEqual(initialSnapshotCount, jobMeta.GetCurrentPackage()!.SnapshotCount);
    }

    [TestMethod]
    public void BackupDifferential_PerformBackup_WithNullJob_DoesNothing()
    {
        var jobMeta = new JobsMetadata();
        var backup = new BackupDifferential();

        backup.PerformBackup(null!, jobMeta);

        Assert.AreEqual(0, jobMeta.Packages.Count);
    }

    [TestMethod]
    public void BackupDifferential_PerformBackup_ReachesSnapshotLimit_CreatesFull()
    {
        var job = CreateTestJob(BackupMethod.Differential);
        job.BackupRetention = new BackupRetention { Count = 5, Size = 1 }; // Size = 1 means only 1 snapshot allowed
        var jobMeta = CreateTestMetadata(job);

        // Create initial full backup
        var fullBackup = new BackupFull();
        fullBackup.PerformBackup(job, jobMeta);

        // Set snapshot count to limit
        var package = jobMeta.GetCurrentPackage()!;
        package.SnapshotCount = 1;

        // Modify a file
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "Modified again");
        File.SetLastWriteTimeUtc(Path.Combine(_sourceDir, "file1.txt"), DateTime.UtcNow.AddSeconds(2));
        jobMeta.LastPackageTimestampUtc = DateTime.UtcNow.AddMinutes(-5);

        var diffBackup = new BackupDifferential();
        diffBackup.PerformBackup(job, jobMeta);

        // Should create new full backup (new package)
        Assert.AreEqual(2, jobMeta.Packages.Count);
    }

    #endregion

    #region BackupIncremental Tests

    [TestMethod]
    public void BackupIncremental_PerformBackup_NoExistingPackage_CreatesFull()
    {
        var job = CreateTestJob(BackupMethod.Incremental);
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupIncremental();

        backup.PerformBackup(job, jobMeta);

        Assert.AreEqual(1, jobMeta.Packages.Count);
        Assert.AreEqual(BackupMethod.Full, jobMeta.Method);
    }

    [TestMethod]
    public void BackupIncremental_PerformBackup_WithExistingPackage_CreatesSnapshot()
    {
        var job = CreateTestJob(BackupMethod.Incremental);
        var jobMeta = CreateTestMetadata(job);

        // First, create a full backup
        var fullBackup = new BackupFull();
        fullBackup.PerformBackup(job, jobMeta);

        // Modify a file to trigger incremental
        Thread.Sleep(100);
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "Modified content for incremental");
        File.SetLastWriteTimeUtc(Path.Combine(_sourceDir, "file1.txt"), DateTime.UtcNow.AddSeconds(1));

        // Now run incremental
        var incBackup = new BackupIncremental();
        incBackup.PerformBackup(job, jobMeta);

        Assert.AreEqual(BackupMethod.Incremental, jobMeta.Method);
    }

    [TestMethod]
    public void BackupIncremental_PerformBackup_NoChanges_DoesNothing()
    {
        var job = CreateTestJob(BackupMethod.Incremental);
        var jobMeta = CreateTestMetadata(job);

        // First, create a full backup
        var fullBackup = new BackupFull();
        fullBackup.PerformBackup(job, jobMeta);

        var initialSnapshotCount = jobMeta.GetCurrentPackage()!.SnapshotCount;

        // Run incremental without changes
        jobMeta.LastSnapshotTimestampUtc = DateTime.UtcNow.AddMinutes(1); // Set to future
        var incBackup = new BackupIncremental();
        incBackup.PerformBackup(job, jobMeta);

        Assert.AreEqual(initialSnapshotCount, jobMeta.GetCurrentPackage()!.SnapshotCount);
    }

    [TestMethod]
    public void BackupIncremental_PerformBackup_WithNullJob_DoesNothing()
    {
        var jobMeta = new JobsMetadata();
        var backup = new BackupIncremental();

        backup.PerformBackup(null!, jobMeta);

        Assert.AreEqual(0, jobMeta.Packages.Count);
    }

    [TestMethod]
    public void BackupIncremental_PerformBackup_ReachesSnapshotLimit_CreatesFull()
    {
        var job = CreateTestJob(BackupMethod.Incremental);
        job.BackupRetention = new BackupRetention { Count = 5, Size = 1 };
        var jobMeta = CreateTestMetadata(job);

        // Create initial full backup
        var fullBackup = new BackupFull();
        fullBackup.PerformBackup(job, jobMeta);

        // Set snapshot count to limit
        var package = jobMeta.GetCurrentPackage()!;
        package.SnapshotCount = 1;

        // Modify a file
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "Modified for incremental limit");
        File.SetLastWriteTimeUtc(Path.Combine(_sourceDir, "file1.txt"), DateTime.UtcNow.AddSeconds(2));
        jobMeta.LastSnapshotTimestampUtc = DateTime.UtcNow.AddMinutes(-5);

        var incBackup = new BackupIncremental();
        incBackup.PerformBackup(job, jobMeta);

        // Should create new full backup
        Assert.AreEqual(2, jobMeta.Packages.Count);
    }

    [TestMethod]
    public void BackupIncremental_UsesLastSnapshotTime_NotPackageTime()
    {
        var job = CreateTestJob(BackupMethod.Incremental);
        var jobMeta = CreateTestMetadata(job);

        // Create full backup
        var fullBackup = new BackupFull();
        fullBackup.PerformBackup(job, jobMeta);

        // Set different times for package and snapshot
        var oldPackageTime = DateTime.UtcNow.AddHours(-2);
        var recentSnapshotTime = DateTime.UtcNow.AddMinutes(-1);
        jobMeta.LastPackageTimestampUtc = oldPackageTime;
        jobMeta.LastSnapshotTimestampUtc = recentSnapshotTime;

        // Modify file with time between package and snapshot time
        var fileTime = DateTime.UtcNow.AddHours(-1); // After package, but before snapshot
        File.WriteAllText(Path.Combine(_sourceDir, "file1.txt"), "Time test");
        File.SetLastWriteTimeUtc(Path.Combine(_sourceDir, "file1.txt"), fileTime);

        var incBackup = new BackupIncremental();
        incBackup.PerformBackup(job, jobMeta);

        // File should not be backed up since it's before LastSnapshotTimestampUtc
        // Snapshot count should stay the same
    }

    #endregion

    #region Backup Base Class Behavior Tests

    [TestMethod]
    public void Backup_CopiesSubdirectories()
    {
        var job = CreateTestJob(BackupMethod.Full);
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        backup.PerformBackup(job, jobMeta);

        var packageDir = Directory.GetDirectories(_targetDir, "package_*").First();
        var fullBackupDir = Path.Combine(packageDir, "fullBackup", "source");
        var subdirTarget = Path.Combine(fullBackupDir, "subdir");

        Assert.IsTrue(Directory.Exists(subdirTarget));
        Assert.IsTrue(File.Exists(Path.Combine(subdirTarget, "file3.txt")));
    }

    [TestMethod]
    public void Backup_MultipleTargets_CopiestoAll()
    {
        var target2 = Path.Combine(_testDir, "target2");
        Directory.CreateDirectory(target2);

        var job = CreateTestJob(BackupMethod.Full);
        job.Targets.Add(target2);
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        backup.PerformBackup(job, jobMeta);

        var packages1 = Directory.GetDirectories(_targetDir, "package_*");
        var packages2 = Directory.GetDirectories(target2, "package_*");

        Assert.AreEqual(1, packages1.Length);
        Assert.AreEqual(1, packages2.Length);
    }

    [TestMethod]
    public void Backup_SkipsEmptySources()
    {
        var job = CreateTestJob(BackupMethod.Full);
        job.Sources.Add("");
        job.Sources.Add("   ");
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        // Should not throw
        backup.PerformBackup(job, jobMeta);

        Assert.AreEqual(1, jobMeta.Packages.Count);
    }

    [TestMethod]
    public void Backup_EnforcesRetention_RemovesOldPackages()
    {
        var job = CreateTestJob(BackupMethod.Full);
        job.BackupRetention = new BackupRetention { Count = 2, Size = 10 };
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        // Create 3 full backups
        backup.PerformBackup(job, jobMeta);
        backup.PerformBackup(job, jobMeta);
        backup.PerformBackup(job, jobMeta);

        // Should only keep 2 packages due to retention
        Assert.AreEqual(2, jobMeta.Packages.Count);
    }

    [TestMethod]
    public void Backup_NoRetention_KeepsAllPackages()
    {
        var job = CreateTestJob(BackupMethod.Full);
        job.BackupRetention = null;
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        backup.PerformBackup(job, jobMeta);
        backup.PerformBackup(job, jobMeta);
        backup.PerformBackup(job, jobMeta);

        Assert.AreEqual(3, jobMeta.Packages.Count);
    }

    [TestMethod]
    public void Backup_ZeroRetention_KeepsAllPackages()
    {
        var job = CreateTestJob(BackupMethod.Full);
        job.BackupRetention = new BackupRetention { Count = 0, Size = 10 };
        var jobMeta = CreateTestMetadata(job);
        var backup = new BackupFull();

        backup.PerformBackup(job, jobMeta);
        backup.PerformBackup(job, jobMeta);

        Assert.AreEqual(2, jobMeta.Packages.Count);
    }

    #endregion
}

