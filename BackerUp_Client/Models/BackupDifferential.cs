using Class_Lib;
using System;
using System.Collections.Generic;
using System.IO;

namespace BackerUp_Client.Models {
    public class BackupDifferential : Backup {
        public override void PerformBackup(BackupJob job, JobsMetadata jobMeta) {
            if (job == null || job.Targets == null || job.Sources == null) {
                return;
            }

            DateTime now = DateTime.UtcNow;

            PackageEntry? current = jobMeta.GetCurrentPackage();
            if (current == null) {
                // If no package exists, perform a full backup
                base.PerformBackup(job, jobMeta);
                return;
            }

            // Differential compares to the last package creation time (start of current package)
            DateTime? last = jobMeta.LastPackageTimestampUtc;
            if (!last.HasValue) {
                base.PerformBackup(job, jobMeta);
                return;
            }

            List<(string root, string path)> changed = new();
            foreach (string src in job.Sources) {
                if (string.IsNullOrWhiteSpace(src)) continue;

                if (Directory.Exists(src)) {
                    foreach (string f in Directory.GetFiles(src, "*", SearchOption.AllDirectories)) {
                        DateTime lw;
                        try { lw = File.GetLastWriteTimeUtc(f); } catch { continue; }
                        if (lw > last.Value) changed.Add((src, f));
                    }
                } else if (File.Exists(src)) {
                    DateTime lw;
                    try { lw = File.GetLastWriteTimeUtc(src); } catch { continue; }
                    if (lw > last.Value) changed.Add((Path.GetDirectoryName(src) ?? "", src));
                }
            }

            if (changed.Count == 0) return;

            current = jobMeta.GetCurrentPackage();
            if (current == null) return;

            int snapshotIndex = current.SnapshotCount;
            int keepSnapshots = job.BackupRetention?.Size > 0 ? job.BackupRetention.Size : 0;

            if (keepSnapshots > 0 && snapshotIndex >= keepSnapshots) {
                // When reaching snapshot limit, perform a full backup (creates new package)
                base.PerformBackup(job, jobMeta);
                return;
            }

            foreach (string target in job.Targets) {
                try {
                    string snapDir = Path.Combine(target, current.Name, $"snapshot_{snapshotIndex}");
                    Directory.CreateDirectory(snapDir);

                    foreach ((string root, string path) pair in changed) {
                        string rel;
                        try { rel = string.IsNullOrEmpty(pair.root) ? Path.GetFileName(pair.path) : Path.GetRelativePath(pair.root, pair.path); } catch { rel = Path.GetFileName(pair.path); }

                        string topFolder = "files";
                        if (!string.IsNullOrEmpty(pair.root) && Directory.Exists(pair.root)) {
                            topFolder = Path.GetFileName(Path.TrimEndingDirectorySeparator(pair.root));
                            if (string.IsNullOrEmpty(topFolder)) topFolder = "files";
                        } else if (!string.IsNullOrEmpty(pair.root)) {
                            topFolder = Path.GetFileName(pair.root);
                            if (string.IsNullOrEmpty(topFolder)) topFolder = "files";
                        }

                        string dest = Path.Combine(snapDir, topFolder, rel);
                        Directory.CreateDirectory(Path.GetDirectoryName(dest) ?? snapDir);
                        File.Copy(pair.path, dest, overwrite: true);
                    }
                } catch (Exception ex) {
                    LoggerService.Log($"Error creating snapshot in package {current.Name} for target {target}: {ex.Message}");
                }
            }

            jobMeta.IncrementSnapshotCount(current.Name);
            jobMeta.LastSnapshotTimestampUtc = now;
            jobMeta.Method = BackupMethod.Differential;
            jobMeta.SaveToAppData();

            EnforceRetention(job, jobMeta);
        }
    }
}
