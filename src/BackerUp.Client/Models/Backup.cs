using BackerUp.Core;
using BackerUp.Core.Models;

namespace BackerUp.Client.Models;

public abstract class Backup
{
    public void Run(BackupJob job)
    {
        JobsMetadata jobMeta = new JobsMetadata();
        try
        {
            jobMeta = JobsMetadata.LoadOrCreateForJob(job);
        }
        catch (Exception ex)
        {
            LoggerService.Log($"Problem with saving metadata for job (Id: {job.Id}): {ex.Message}");
        }

        // Check cron/timing to determine if job should run now
#if DEBUG
#else
        if (!TimingService.IsJobDue(job, jobMeta))
        {
            LoggerService.Log($"Job (Id: {job.Id}) is not due yet.");
            return;
        }
#endif

        LoggerService.Log($"Job {job.Id} is due. Starting backup...");
        PerformBackup(job, jobMeta);
    }

    public virtual void PerformBackup(BackupJob job, JobsMetadata jobMeta)
    {
        // Default implementation performs a full backup
        if (job == null || job.Targets == null || job.Sources == null)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;

        List<string> validSources = new();
        foreach (string source in job.Sources)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                LoggerService.Log($"Job {job.Id} has an empty source path and it was skipped.");
                continue;
            }

            if (!Directory.Exists(source) && !File.Exists(source))
            {
                LoggerService.Log($"Job {job.Id} source path does not exist: {source}");
                continue;
            }

            validSources.Add(source);
        }

        if (validSources.Count == 0)
        {
            LoggerService.Log($"Job {job.Id} has no valid source paths, skipping backup.");
            return;
        }

        // Full backups always create a new package
        CreateNewPackageForJob(job, jobMeta);
        PackageEntry? current = jobMeta.GetCurrentPackage();
        if (current == null)
        {
            return;
        }

        foreach (string target in job.Targets)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(target))
                {
                    LoggerService.Log($"Job {job.Id} has an empty target path and it was skipped.");
                    continue;
                }

                if (!Directory.Exists(target))
                {
                    LoggerService.Log($"Job {job.Id} target path does not exist and was skipped: {target}");
                    continue;
                }

                string dataDir = Path.Combine(target, current.Name, "fullBackup");
                Directory.CreateDirectory(dataDir);
                foreach (string src in validSources)
                {
                    string dest = Path.Combine(dataDir, Path.GetFileName(src.TrimEnd(Path.DirectorySeparatorChar)));
                    CopyPathPreserve(src, dest);
                }
            }
            catch (Exception ex)
            {
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

    protected void CreateNewPackageForJob(BackupJob job, JobsMetadata jobMeta)
    {
        int pkgIndex = jobMeta.NextPackageIndex;
        string packageBase = $"package_{job.Id}_{pkgIndex}";
        DateTime now = DateTime.Now;
        foreach (string target in job.Targets)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(target))
                {
                    LoggerService.Log($"Job {job.Id} has an empty target path and it was skipped.");
                    continue;
                }

                if (!Directory.Exists(target))
                {
                    LoggerService.Log($"Job {job.Id} target path does not exist and was skipped: {target}");
                    continue;
                }

                // Create package base directory only. The actual data will be written to the 'fullBackup' folder when a full backup runs.
                string packageDir = Path.Combine(target, packageBase);
                Directory.CreateDirectory(packageDir);
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Error creating package {packageBase} in target {target}: {ex.Message}");
            }
        }

        jobMeta.AddPackage(packageBase, now);
        jobMeta.NextPackageIndex += 1;
        jobMeta.LastPackageTimestampUtc = now;
        jobMeta.SaveToAppData();
    }

    protected void CopyPathPreserve(string source, string destination)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return;
        }

        if (Directory.Exists(source))
        {
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, dirPath)));
            }
            foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                string rel = Path.GetRelativePath(source, file);
                string dest = Path.Combine(destination, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dest) ?? destination);
                File.Copy(file, dest, overwrite: true);
            }
        }
        else if (File.Exists(source))
        {
            string? destinationDirectory = Path.GetDirectoryName(destination);
            if (string.IsNullOrWhiteSpace(destinationDirectory))
            {
                LoggerService.Log($"Could not resolve destination directory for source {source}.");
                return;
            }

            Directory.CreateDirectory(destinationDirectory);
            File.Copy(source, destination, overwrite: true);
            return;
        }

        LoggerService.Log($"Source path disappeared before it could be copied: {source}");
    }

    protected bool TryCreateTargetPath(string jobId, string target, string relativePath, out string resolvedPath)
    {
        resolvedPath = string.Empty;

        if (string.IsNullOrWhiteSpace(target))
        {
            LoggerService.Log($"Job {jobId} has an empty target path and it was skipped.");
            return false;
        }

        try
        {
            resolvedPath = Path.Combine(target, relativePath);
            Directory.CreateDirectory(resolvedPath);
            return true;
        }
        catch (Exception ex)
        {
            LoggerService.Log($"Job {jobId} target path is invalid or could not be created: {target}. {ex.Message}");
            return false;
        }
    }

    protected void EnforceRetention(BackupJob job, JobsMetadata jobMeta)
    {
        if (job.BackupRetention == null)
        {
            return;
        }

        int keep = job.BackupRetention.Count;
        if (keep <= 0)
        {
            return;
        }

        List<string> removed = jobMeta.PurgeOldPackages(keep);
        foreach (string pkg in removed)
        {
            foreach (string target in job.Targets)
            {
                try
                {
                    string dir = Path.Combine(target, pkg);
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, recursive: true);
                    }
                }
                catch { }
            }
        }

        jobMeta.SaveToAppData();
    }
}
