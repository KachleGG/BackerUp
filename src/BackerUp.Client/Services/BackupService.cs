using BackerUp.Client.Models;
using BackerUp.Core;

namespace BackerUp.Client.Services;

public class BackupService {
    public List<BackupJob> BackupJobs { get; set; }
    public BackupService(List<BackupJob> backupJobs) {
        BackupJobs = backupJobs;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default) {
        if (BackupJobs == null || BackupJobs.Count == 0) {
            return;
        }

        List<Task> tasks = new();
        for (int i = 0; i < BackupJobs.Count; i++) {
            BackupJob job = BackupJobs[i];
            if (job == null) {
                continue;
            }

            tasks.Add(Task.Run(() => { 
                if (!cancellationToken.IsCancellationRequested) { 
                    PerformBackupIfDue(job); 
                } 
            }, cancellationToken));
        }

        try { 
            await Task.WhenAll(tasks).ConfigureAwait(false); 
        } catch (OperationCanceledException) { }
    }

    private void PerformBackupIfDue(BackupJob job) {
        if (job == null) {
            return;
        }

        // Create the appropriate Backup implementation based on the job method
        Backup backupImpl = job.Method switch {
            BackupMethod.Full => new BackupFull(),
            BackupMethod.Differential => new BackupDifferential(),
            BackupMethod.Incremental => new BackupIncremental(),
            _ => new BackupFull(),
        };

        try {
            backupImpl.Run(job);
        } catch (Exception ex) {
            LoggerService.Log($"Error running backup for job {job.Id}: {ex.Message}");
        }
    }
}
