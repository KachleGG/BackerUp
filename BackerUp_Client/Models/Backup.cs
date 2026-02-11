using BackerUp_Client.Services;
using Class_Lib;

namespace BackerUp_Client.Models;

public abstract class Backup
{
    public void Run(BackupJob job) {
        JobsMetadata jobMeta = new JobsMetadata();
        try {
            jobMeta = JobsMetadata.LoadOrCreateForJob(job);
        } catch (Exception ex) {
            LoggerService.Log($"Problem with saving metadata for job (Id: {job.Id}): {ex.Message}");
        }

        // Check cron/timing to determine if job should run now
#if DEBUG
#else
        if (!CronTimingService.IsJobDue(job, jobMeta))
        {
            LoggerService.Log($"Job (Id: {job.Id}) is not due yet.");
            return;
        }
#endif

        LoggerService.Log($"Job {job.Id} is due. Starting backup...");
        PerformBackup(job, jobMeta);
    }

    public virtual void PerformBackup(BackupJob job, JobsMetadata jobMeta) {
        // Default implementation performs a full backup
        if (job == null || job.Targets == null || job.Sources == null) {
            return;
        }

        DateTime now = DateTime.UtcNow;

        // Full backups always create a new package
        CreateNewPackageForJob(job, jobMeta);
        PackageEntry? current = jobMeta.GetCurrentPackage();
        if (current == null) {
            return;
        }

        foreach (string target in job.Targets) {
            try {
                string dataDir = Path.Combine(target, current.Name, "fullBackup");
                Directory.CreateDirectory(dataDir);
                foreach (string src in job.Sources) {
                    if (string.IsNullOrWhiteSpace(src)) {
                        continue;
                    }

                    string dest = Path.Combine(dataDir, Path.GetFileName(src.TrimEnd(Path.DirectorySeparatorChar)));
                    CopyPathPreserve(src, dest);
                }
            } catch (Exception ex) {
                LoggerService.Log($"Error copying data for full package {current.Name} into target {target}: {ex.Message}");
            }
        }

        jobMeta.IncrementSnapshotCount(current.Name);
        jobMeta.LastSnapshotTimestampUtc = now;
        jobMeta.LastPackageTimestampUtc = now;
        jobMeta.Method = BackupMethod.Full;
        jobMeta.SaveToAppData();
        EnforceRetention(job, jobMeta);
    }

    protected void CreateNewPackageForJob(BackupJob job, JobsMetadata jobMeta) {
        int pkgIndex = jobMeta.NextPackageIndex;
        string packageBase = $"package_{job.Id}_{pkgIndex}";
        DateTime now = DateTime.Now;
        foreach (string target in job.Targets) {
            try {
                // Create package base directory only. The actual data will be written to the 'fullBackup' folder when a full backup runs.
                string packageDir = Path.Combine(target, packageBase);
                Directory.CreateDirectory(packageDir);
            } catch (Exception ex) {
                LoggerService.Log($"Error creating package {packageBase} in target {target}: {ex.Message}");
            }
        }

        jobMeta.AddPackage(packageBase, now);
        jobMeta.NextPackageIndex += 1;
        jobMeta.LastPackageTimestampUtc = now;
        jobMeta.SaveToAppData();
    }

    protected void CopyPathPreserve(string source, string destination) {
        if (Directory.Exists(source)) {
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories)) {
                Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, dirPath)));
            }
            foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories)) {
                string rel = Path.GetRelativePath(source, file);
                string dest = Path.Combine(destination, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dest) ?? destination);
                File.Copy(file, dest, overwrite: true);
            }
        } else if (File.Exists(source)) {
            Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? "");
            File.Copy(source, destination, overwrite: true);
        }
    }

    protected void EnforceRetention(BackupJob job, JobsMetadata jobMeta) {
        if (job.BackupRetention == null) {
            return;
        }

        int keep = job.BackupRetention.Count;
        if (keep <= 0) {
            return;
        }

        List<string> removed = jobMeta.PurgeOldPackages(keep);
        foreach (string pkg in removed) {
            foreach (string target in job.Targets) {
                try {
                    string dir = Path.Combine(target, pkg);
                    if (Directory.Exists(dir)) {
                        Directory.Delete(dir, recursive: true);
                    }
                } catch { }
            }
        }

        jobMeta.SaveToAppData();
    }
}
